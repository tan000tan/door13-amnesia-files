#!/bin/bash
# 《第十三扇门：失忆档案》自动打包流水线
# 用法: ./build_door13.sh [all|compile|setup|smoke|apk]
#   all    = 联接 + 编译 + 配置 + 冒烟测试 + 打包 + 交付改名（默认）
#   单步骤 = 只执行该步（假定 D:\TDM 联接已存在）
#
# 关键背景：安卓构建工具不支持中文路径，
# 工程真实路径在 D:\Agent工作区\ZCode工作区\第十三扇门_失忆档案，
# 所有 Unity 会话一律通过 ASCII 联接 D:\TDM 打开。
set -u

ROOT="D:/Agent工作区/ZCode工作区/第十三扇门_失忆档案"
PROJ="$ROOT"
UNITY="D:/Agent工作区/Unity开发环境/UnityEditors/Unity2022.3.62f3c1/Editor/Unity.exe"
JUNC_PS="D:\\TDM"
JUNC_U="/d/TDM"
LOGDIR="$ROOT/Tools/logs"
OUTDIR="$ROOT/Builds"
APK_NAME="第十三扇门_失忆档案_Final.apk"
STEP="${1:-all}"

mkdir -p "$LOGDIR" "$OUTDIR"

ensure_junction() {
  if [ -d "$JUNC_U" ]; then echo "[联接] D:\\TDM 已存在"; return 0; fi
  echo "[联接] 创建 D:\\TDM -> UnityProject ..."
  local CMD="New-Item -ItemType Junction -Path 'D:\\TDM' -Target 'D:\\Agent工作区\\ZCode工作区\\第十三扇门_失忆档案' -Force | Out-Null; if (Test-Path 'D:\\TDM') { 'JUNCTION_OK' } else { 'JUNCTION_FAIL' }"
  local ENC R
  ENC=$(printf '%s' "$CMD" | iconv -f UTF-8 -t UTF-16LE | base64 -w0)
  R=$(powershell -NoProfile -EncodedCommand "$ENC")
  [ "$R" = "JUNCTION_OK" ] || { echo "联接创建失败: $R"; exit 9; }
  echo "[联接] 创建成功"
}

run_unity() { # $1=日志名 其余=Unity参数
  local log="$1"; shift
  "$UNITY" -batchmode -quit -nographics -projectPath "D:/TDM" -logFile "$LOGDIR/$log" "$@"
  local rc=$?
  echo "  [$log] 退出码: $rc"
  return $rc
}

run_unity_play() { # 无 -quit：播放模式冒烟测试专用，由 SmokeTest 内部 EditorApplication.Exit 结束
  local log="$1"; shift
  "$UNITY" -batchmode -nographics -projectPath "D:/TDM" -logFile "$LOGDIR/$log" "$@"
  local rc=$?
  echo "  [$log] 退出码: $rc"
  return $rc
}

do_compile() {
  echo "[1/5] 编译检查..."
  run_unity 2_compile.log || { echo "编译会话异常退出"; exit 1; }
  if grep -qiE "error CS" "$LOGDIR/2_compile.log"; then
    grep -iE "error CS" "$LOGDIR/2_compile.log" | head -20
    echo "存在编译错误！"; exit 1
  fi
  echo "  编译通过"
}

do_setup() {
  echo "[2/5] 配置+生成测试场景..."
  run_unity 3_setup.log -executeMethod Door13.EditorTools.BuildDoor13.SetupOnly || exit 1
  grep -E "SCENE_OK|TOOLSETTING" "$LOGDIR/3_setup.log" | head -8
}

do_smoke() {
  echo "[3/5] 播放模式冒烟测试..."
  run_unity_play 4_smoke.log -executeMethod Door13.EditorTools.SmokeTest.Run || { echo "冒烟测试未通过！"; exit 1; }
  grep -E "SMOKE_RESULT" "$LOGDIR/4_smoke.log" || { echo "未见 SMOKE_RESULT，视为未通过！"; exit 1; }
}

do_apk() {
  echo "[4/5] 打包APK（IL2CPP，需数分钟）..."
  run_unity 5_build.log -executeMethod Door13.EditorTools.BuildDoor13.BuildApkOnly || { echo "打包失败"; exit 1; }
  grep -E "BUILD_RESULT" "$LOGDIR/5_build.log"
  [ -f "$PROJ/Build/Door13.apk" ] || { echo "APK未生成，请检查 $LOGDIR/5_build.log"; exit 1; }
}

do_deliver() {
  echo "[5/5] 交付改名..."
  cp -f "$PROJ/Build/Door13.apk" "$OUTDIR/$APK_NAME"
  ls -la "$OUTDIR/$APK_NAME"
}

case "$STEP" in
  all)     ensure_junction; do_compile; do_setup; do_smoke; do_apk; do_deliver; echo "=== 流水线全部完成 ===";;
  compile) do_compile;;
  setup)   do_setup;;
  smoke)   do_smoke;;
  apk)     do_apk; do_deliver;;
  *)       echo "用法: $0 [all|compile|setup|smoke|apk]"; exit 64;;
esac
