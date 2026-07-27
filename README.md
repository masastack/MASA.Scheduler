<p align="center">
  <a href="https://www.masastack.com/stack" target="_blank">
    <img alt="Logo" src="https://cdn.masastack.com/images/Scheduler.png">
  </a>
</p>

## 介绍

Scheduler是MASA Stack 1.0推出的一款辅助性软件产品，主要负责处理应用程序任务执行的调度，以及自动重试等相关操作。在MASA Stack产品中，与MASA MC、MASA TSC、MASA Alert 3款产品结合，发挥最大的调度价值。当然Scheduler并不只是给MASA Stack产品使用，它同样可以为业务创造价值。
![Scheduler](http://cdn.masastack.com/stack/doc/scheduler/introduce.png)

## MASA Stack 概览
[传送门](https://github.com/masastack/MASA.Stack)

## 特性
- 待补充

## 安装
- 待补充


## 本地开发
- Dapr Jobs 支持需 Dapr 1.14+ 并启用 Scheduler control plane
- 服务端配置切换：`Scheduler:Backend` 设为 `DaprJobs`
- Jobs 回调路径固定为 `/job/{name}`，由服务端接收并触发任务

## Dapr Jobs 时区（北京时间）
- `DaprJobs` 的 cron 由 `dapr-scheduler` 所在服务器本地时区解释，不支持在 cron 里写 `CRON_TZ=` / `TZ=` 前缀。
- 如需统一按北京时间触发，请将 scheduler 控制面时区设为 `Asia/Shanghai`。
- 在 Dapr 1.18 的 Kubernetes 部署中，scheduler 通常是 StatefulSet：`dapr-scheduler-server`（例如 `dapr-scheduler-server-0/1/2`）。
- 仓库提供了运维脚本：
  - 设置并发布：`dapr/ops/set-dapr-scheduler-timezone.ps1`
  - 快速验证：`dapr/ops/verify-dapr-scheduler-timezone.ps1`
- 仓库提供了 GitOps/Helm 示例：
  - K8s Patch：`dapr/ops/k8s/dapr-scheduler-timezone-patch.yaml`
  - Helm Values：`dapr/ops/helm/dapr-control-plane-values.timezone.yaml`
  - 触发验证清单：`dapr/ops/verify-dapr-cron-bjt-checklist.md`
- 示例（PowerShell）：
  - `./dapr/ops/set-dapr-scheduler-timezone.ps1 -Namespace dapr-system -StatefulSetName dapr-scheduler-server -TimeZone Asia/Shanghai`
  - `./dapr/ops/verify-dapr-scheduler-timezone.ps1 -Namespace dapr-system -StatefulSetName dapr-scheduler-server`
- 示例（GitOps/Kustomize）：
  - 将 `dapr/ops/k8s/dapr-scheduler-timezone-patch.yaml` 复制到你的集群配置仓库 overlay。
  - 在 overlay 的 `kustomization.yaml` 中对 `StatefulSet/dapr-scheduler-server` 引用该 patch。
- 示例（Helm）：
  - `helm upgrade --install dapr dapr/dapr --namespace dapr-system --values dapr/ops/helm/dapr-control-plane-values.timezone.yaml`
- 验证建议：
  - 确认 StatefulSet 模板中存在 `TZ=Asia/Shanghai`。
  - 对全部 scheduler Pod 执行 `date`，确保均为 `+0800`。
  - 注册“下一个整分钟”的测试 cron，确认实际触发时间不再偏移 8 小时。

## 开发路线
- 待补充

## 贡献者

感谢所有为本项目做出过贡献的朋友。

<a href="https://github.com/masastack/MASA.Scheduler/graphs/contributors"> 
    <img src="https://contrib.rocks/image?repo=masastack/MASA.Scheduler" /> 
</a>

## 行为准则

本项目采用了《贡献者公约》所定义的行为准则，以明确我们社区的预期行为。
更多信息请见 [MASA Stack Community Code of Conduct](https://github.com/masastack/community/blob/main/CODE-OF-CONDUCT.md).

