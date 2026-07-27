# Dapr Jobs 北京时间触发验证清单（StatefulSet）

## 1) 控制面时区验证

```bash
kubectl -n dapr-system get sts dapr-scheduler-server -o yaml | grep -A5 -n "name: TZ"
kubectl -n dapr-system get pod -l app.kubernetes.io/name=dapr-scheduler -o name
kubectl -n dapr-system exec dapr-scheduler-server-0 -- date
kubectl -n dapr-system exec dapr-scheduler-server-1 -- date
kubectl -n dapr-system exec dapr-scheduler-server-2 -- date
```

预期结果：3 个 Pod 都显示 `+0800`，且模板里存在 `TZ=Asia/Shanghai`。

## 2) 快速触发验证（是否仍偏移 8 小时）

1. 取北京时间“当前小时 + 下一分钟”生成 cron：

```bash
date "+当前北京时间: %Y-%m-%d %H:%M:%S %z"
# 假设当前是 16:08，则测试 cron 用：0 9 16 * * *
```

2. 在 Scheduler 管理端创建一个测试 cron 任务（表达式示例：`0 9 16 * * *`）。
3. 观察任务回调日志（服务端 `/job/{name}` 的处理日志）是否在下一分钟触发。

判定：
- 若下一分钟触发：北京时间生效。
- 若不触发，且约 8 小时后才触发：仍按 UTC 解释。

## 3) 回传模板（发给研发）

请回传以下信息：

- `kubectl -n dapr-system exec dapr-scheduler-server-0 -- date` 输出：
- `kubectl -n dapr-system exec dapr-scheduler-server-1 -- date` 输出：
- `kubectl -n dapr-system exec dapr-scheduler-server-2 -- date` 输出：
- 测试 cron 表达式：
- 任务创建时间（北京时间）：
- 实际首次触发时间（北京时间）：
- 偏移结论（正常/偏移 8 小时）：
