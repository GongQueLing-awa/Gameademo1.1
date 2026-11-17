
## 2026-09-23 22:17:08 ProjectAssetData 全量重建基线

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `38`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 14.29 | 14.01 | 1.0 | 11125529 | 100.8458 | 98.8597 |
| `PADI_ProcessBatch` | 24.47 | 22.17 | 1.0 | 388917 | 5.2330 | 4.7411 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-23 22:20:08 ProjectAssetData 全量重建基线

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `43`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 10.25 | 10.02 | 1.0 | 11125529 | 96.2834 | 94.1340 |
| `PADI_FullScan` | 10.06 | 6.51 | 1.0 | 4783918 | 94.5053 | 61.1207 |
| `PADI_ProcessBatch` | 17.70 | 15.98 | 1.0 | 388917 | 5.0365 | 4.5482 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-23 22:21:30 ProjectAssetData 增量重建基线

- 图纸: `ProjectAssetData 单资源增量索引重建` (project-asset-index-incremental-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `3`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 50.87 | 50.74 | 1.0 | 994258 | 334.5934 | 333.7629 |
| `PADI_IncrementalScan` | 40.79 | 33.80 | 1.0 | 13373724 | 268.2877 | 222.3194 |
| `PADI_ProcessBatch` | 0.84 | 0.06 | 1.0 | 1920 | 5.5462 | 0.3665 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-23 23:35:31 binary-snapshot-full

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `261`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_FullScan` | 1.90 | 1.32 | 1.0 | 7120862 | 142.6160 | 99.0260 |
| `PADI_SaveSnapshot` | 1.74 | 1.70 | 1.0 | 1553918 | 130.8402 | 127.5363 |
| `PADI_ProcessBatch` | 81.12 | 24.11 | 1.0 | 78221 | 23.7364 | 7.0552 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-23 23:37:29 change-list-incremental

- 图纸: `ProjectAssetData 单资源增量索引重建` (project-asset-index-incremental-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `10`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 32.50 | 32.43 | 1.0 | 1552918 | 163.9427 | 163.5755 |
| `PADI_IncrementalScan` | 2.02 | 1.89 | 1.0 | 244302 | 10.1960 | 9.5136 |
| `PADI_ProcessBatch` | 0.14 | 0.07 | 1.0 | 1792 | 0.7110 | 0.3674 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-23 23:45:59 base-snapshot-full

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `157`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_FullScan` | 2.60 | 1.78 | 1.0 | 7120862 | 114.3990 | 78.2016 |
| `PADI_SaveSnapshot` | 2.56 | 2.48 | 1.0 | 1553214 | 112.7104 | 109.2056 |
| `PADI_ProcessBatch` | 71.27 | 39.14 | 1.0 | 136753 | 21.3403 | 11.7189 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.scenarios[0].samples[2].GC Alloc` | 78221.0000 | 136753.0000 | 58532.0000 | 74.83% | 负收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 0.3800 | 0.6400 | 0.2600 | 68.42% | 负收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 0.3800 | 0.6400 | 0.2600 | 68.42% | 负收益 |
| `$.scenarios[0].samples[2].Self ms` | 7.0552 | 11.7189 | 4.6637 | 66.10% | 负收益 |
| `$.scenarios[0].samples[2].Self` | 24.1100 | 39.1400 | 15.0300 | 62.34% | 负收益 |
| `$.scenarios[0].samples[1].Total` | 1.7400 | 2.5600 | 0.8200 | 47.13% | 负收益 |
| `$.scenarios[0].samples[1].Self` | 1.7000 | 2.4800 | 0.7800 | 45.88% | 负收益 |
| `$.scenarios[0].samples[2].Hit Frames` | 257.0000 | 147.0000 | -110.0000 | -42.80% | 正收益 |
| `$.scenarios[0].samples[2].Last Hit Frame` | 258.0000 | 148.0000 | -110.0000 | -42.64% | 正收益 |
| `$.scenarios[0].samples[1].Last Hit Frame` | 258.0000 | 148.0000 | -110.0000 | -42.64% | 正收益 |
| `$.scenarios[0].samples[1].First Hit Frame` | 258.0000 | 148.0000 | -110.0000 | -42.64% | 正收益 |
| `$.lastFrameIndex` | 260.0000 | 156.0000 | -104.0000 | -40.00% | 正收益 |
| `$.totalFrames` | 261.0000 | 157.0000 | -104.0000 | -39.85% | 正收益 |
| `$.scenarios[0].measuredFrames` | 261.0000 | 157.0000 | -104.0000 | -39.85% | 正收益 |
| `$.scenarios[0].samples[0].Total` | 1.9000 | 2.6000 | 0.7000 | 36.84% | 负收益 |
| `$.scenarios[0].samples[2].Peak Time ms Per Frame` | 138.4523 | 89.1226 | -49.3297 | -35.63% | 正收益 |
| `$.scenarios[0].samples[0].Self` | 1.3200 | 1.7800 | 0.4600 | 34.85% | 负收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 99.0260 | 78.2016 | -20.8244 | -21.03% | 正收益 |
| `$.scenarios[0].samples[0].Self ms` | 99.0260 | 78.2016 | -20.8244 | -21.03% | 正收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 142.6160 | 114.3990 | -28.2170 | -19.79% | 正收益 |
| `$.scenarios[0].samples[0].Time ms` | 142.6160 | 114.3990 | -28.2170 | -19.79% | 正收益 |
| `$.scenarios[0].samples[1].Self ms` | 127.5363 | 109.2056 | -18.3307 | -14.37% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 127.5363 | 109.2056 | -18.3307 | -14.37% | 正收益 |
| `$.scenarios[0].samples[1].Time ms` | 130.8402 | 112.7104 | -18.1298 | -13.86% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 130.8402 | 112.7104 | -18.1298 | -13.86% | 正收益 |
| `$.scenarios[0].samples[2].Total` | 81.1200 | 71.2700 | -9.8500 | -12.14% | 正收益 |
| `$.scenarios[0].samples[2].Peak Self ms Per Frame` | 45.1422 | 49.9697 | 4.8275 | 10.69% | 负收益 |
| `$.scenarios[0].samples[2].Time ms` | 23.7364 | 21.3403 | -2.3961 | -10.09% | 正收益 |
| `$.scenarios[0].samples[2].Hit Frame Ratio` | 98.4700 | 93.6300 | -4.8400 | -4.92% | 正收益 |
| `$.scenarios[0].samples[1].GC Alloc` | 1553918.0000 | 1553214.0000 | -704.0000 | -0.05% | 持平 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 1553918.0000 | 1553214.0000 | -704.0000 | -0.05% | 持平 |

### 收益判定

**结果：正收益** — 均值指标（Time ms / GC Alloc）改善居多，可保留代码修改。

---

## 2026-09-23 23:46:30 append-incremental

- 图纸: `ProjectAssetData 单资源增量索引重建` (project-asset-index-incremental-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `9`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_IncrementalScan` | 3.52 | 3.23 | 1.0 | 244366 | 12.5845 | 11.5304 |
| `PADI_SaveSnapshot` | 0.52 | 0.24 | 1.0 | 8280 | 1.8604 | 0.8634 |
| `PADI_ProcessBatch` | 0.16 | 0.09 | 1.0 | 1792 | 0.5567 | 0.3204 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.scenarios[0].samples[1].Last Hit Frame` | 1.0000 | 2.0000 | 1.0000 | 100.00% | 负收益 |
| `$.scenarios[0].samples[1].First Hit Frame` | 1.0000 | 2.0000 | 1.0000 | 100.00% | 负收益 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 244302.0000 | 8280.0000 | -236022.0000 | -96.61% | 正收益 |
| `$.scenarios[0].samples[1].GC Alloc` | 244302.0000 | 8280.0000 | -236022.0000 | -96.61% | 正收益 |
| `$.scenarios[0].samples[0].Self ms` | 163.5755 | 11.5304 | -152.0451 | -92.95% | 正收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 163.5755 | 11.5304 | -152.0451 | -92.95% | 正收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 163.9427 | 12.5845 | -151.3582 | -92.32% | 正收益 |
| `$.scenarios[0].samples[0].Time ms` | 163.9427 | 12.5845 | -151.3582 | -92.32% | 正收益 |
| `$.scenarios[0].samples[1].Self ms` | 9.5136 | 0.8634 | -8.6502 | -90.92% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 9.5136 | 0.8634 | -8.6502 | -90.92% | 正收益 |
| `$.scenarios[0].samples[0].Self` | 32.4300 | 3.2300 | -29.2000 | -90.04% | 正收益 |
| `$.scenarios[0].samples[0].Total` | 32.5000 | 3.5200 | -28.9800 | -89.17% | 正收益 |
| `$.scenarios[0].samples[1].Self` | 1.8900 | 0.2400 | -1.6500 | -87.30% | 正收益 |
| `$.scenarios[0].samples[0].Peak GC Alloc Per Frame` | 1552918.0000 | 244366.0000 | -1308552.0000 | -84.26% | 正收益 |
| `$.scenarios[0].samples[0].GC Alloc` | 1552918.0000 | 244366.0000 | -1308552.0000 | -84.26% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 10.1960 | 1.8604 | -8.3356 | -81.75% | 正收益 |
| `$.scenarios[0].samples[1].Time ms` | 10.1960 | 1.8604 | -8.3356 | -81.75% | 正收益 |
| `$.scenarios[0].samples[1].Total` | 2.0200 | 0.5200 | -1.5000 | -74.26% | 正收益 |
| `$.scenarios[0].samples[0].Last Hit Frame` | 2.0000 | 1.0000 | -1.0000 | -50.00% | 正收益 |
| `$.scenarios[0].samples[0].First Hit Frame` | 2.0000 | 1.0000 | -1.0000 | -50.00% | 正收益 |
| `$.scenarios[0].samples[2].Self` | 0.0700 | 0.0900 | 0.0200 | 28.57% | 负收益 |
| `$.scenarios[0].samples[2].Peak Time ms Per Frame` | 0.7110 | 0.5567 | -0.1543 | -21.70% | 正收益 |
| `$.scenarios[0].samples[2].Time ms` | 0.7110 | 0.5567 | -0.1543 | -21.70% | 正收益 |
| `$.scenarios[0].samples[2].Total` | 0.1400 | 0.1600 | 0.0200 | 14.29% | 负收益 |
| `$.scenarios[0].samples[2].Self ms` | 0.3674 | 0.3204 | -0.0470 | -12.79% | 正收益 |
| `$.scenarios[0].samples[2].Peak Self ms Per Frame` | 0.3674 | 0.3204 | -0.0470 | -12.79% | 正收益 |
| `$.lastFrameIndex` | 9.0000 | 8.0000 | -1.0000 | -11.11% | 正收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 10.0000 | 11.1100 | 1.1100 | 11.10% | 负收益 |
| `$.scenarios[0].samples[2].Hit Frame Ratio` | 10.0000 | 11.1100 | 1.1100 | 11.10% | 负收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 10.0000 | 11.1100 | 1.1100 | 11.10% | 负收益 |
| `$.scenarios[0].measuredFrames` | 10.0000 | 9.0000 | -1.0000 | -10.00% | 正收益 |
| `$.totalFrames` | 10.0000 | 9.0000 | -1.0000 | -10.00% | 正收益 |

### 收益判定

**结果：正收益** — 均值指标（Time ms / GC Alloc）改善居多，可保留代码修改。

---

## 2026-09-24 07:39:05 path-order-full

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `300`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 1.18 | 1.15 | 1.0 | 1553254 | 107.9438 | 104.7315 |
| `PADI_ProcessBatch` | 81.80 | 13.86 | 1.0 | 31155 | 25.3938 | 4.3033 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-24 07:39:31 path-order-incremental

- 图纸: `ProjectAssetData 单资源增量索引重建` (project-asset-index-incremental-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `4`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 1.48 | 0.79 | 1.0 | 8560 | 1.8089 | 0.9631 |
| `PADI_IncrementalScan` | 0.85 | 0.23 | 1.0 | 1338 | 1.0461 | 0.2792 |
| `PADI_ProcessBatch` | 0.44 | 0.24 | 1.0 | 1792 | 0.5372 | 0.2961 |

### 收益判定

**结果：首次测试（无 baseline）** — 门禁通过后本次结果将保存为 baseline。

---

## 2026-09-24 07:56:44 marker-split-full

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `51`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 9.74 | 9.53 | 1.0 | 1553206 | 102.5378 | 100.3751 |
| `PADI_ClassifyPaths` | 7.58 | 4.80 | 1.0 | 3782956 | 79.8667 | 50.5725 |
| `PADI_FindAssets` | 2.20 | 2.02 | 1.0 | 3136970 | 23.1742 | 21.2769 |
| `PADI_ProcessBatch` | 47.21 | 45.19 | 1.0 | 364184 | 10.8061 | 10.3432 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.scenarios[0].samples[1].GC Alloc` | 31155.0000 | 3782956.0000 | 3751801.0000 | 12042.37% | 负收益 |
| `$.scenarios[0].samples[1].Self ms` | 4.3033 | 50.5725 | 46.2692 | 1075.20% | 负收益 |
| `$.scenarios[0].samples[0].Self` | 1.1500 | 9.5300 | 8.3800 | 728.70% | 负收益 |
| `$.scenarios[0].samples[0].Total` | 1.1800 | 9.7400 | 8.5600 | 725.42% | 负收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 0.3300 | 1.9600 | 1.6300 | 493.94% | 负收益 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 1047916.0000 | 3782956.0000 | 2735040.0000 | 261.00% | 负收益 |
| `$.scenarios[0].samples[1].Time ms` | 25.3938 | 79.8667 | 54.4729 | 214.51% | 负收益 |
| `$.scenarios[0].samples[1].First Hit Frame` | 0.0000 | 2.0000 | 2.0000 | 100.00% | 负收益 |
| `$.scenarios[0].samples[1].Hit Frames` | 294.0000 | 1.0000 | -293.0000 | -99.66% | 正收益 |
| `$.scenarios[0].samples[1].Last Hit Frame` | 293.0000 | 2.0000 | -291.0000 | -99.32% | 正收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 98.0000 | 1.9600 | -96.0400 | -98.00% | 正收益 |
| `$.scenarios[0].samples[1].Total` | 81.8000 | 7.5800 | -74.2200 | -90.73% | 正收益 |
| `$.scenarios[0].samples[0].First Hit Frame` | 293.0000 | 48.0000 | -245.0000 | -83.62% | 正收益 |
| `$.scenarios[0].samples[0].Last Hit Frame` | 293.0000 | 48.0000 | -245.0000 | -83.62% | 正收益 |
| `$.lastFrameIndex` | 299.0000 | 50.0000 | -249.0000 | -83.28% | 正收益 |
| `$.scenarios[0].measuredFrames` | 300.0000 | 51.0000 | -249.0000 | -83.00% | 正收益 |
| `$.totalFrames` | 300.0000 | 51.0000 | -249.0000 | -83.00% | 正收益 |
| `$.scenarios[0].samples[1].Self` | 13.8600 | 4.8000 | -9.0600 | -65.37% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 143.4912 | 79.8667 | -63.6245 | -44.34% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 46.7829 | 50.5725 | 3.7896 | 8.10% | 负收益 |
| `$.scenarios[0].samples[0].Time ms` | 107.9438 | 102.5378 | -5.4060 | -5.01% | 正收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 107.9438 | 102.5378 | -5.4060 | -5.01% | 正收益 |
| `$.scenarios[0].samples[0].Self ms` | 104.7315 | 100.3751 | -4.3564 | -4.16% | 正收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 104.7315 | 100.3751 | -4.3564 | -4.16% | 正收益 |
| `$.scenarios[0].samples[0].Peak GC Alloc Per Frame` | 1553254.0000 | 1553206.0000 | -48.0000 | 0.00% | 持平 |
| `$.scenarios[0].samples[0].GC Alloc` | 1553254.0000 | 1553206.0000 | -48.0000 | 0.00% | 持平 |

### 收益判定

**结果：负收益** — 均值指标（Time ms / GC Alloc）劣化居多，建议回撤代码修改。

---

## 2026-09-24 07:58:43 slice-classify-full

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `133`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 2.11 | 2.07 | 1.0 | 1552918 | 108.4149 | 106.3218 |
| `PADI_FindAssets` | 0.44 | 0.40 | 1.0 | 3136970 | 22.4199 | 20.5635 |
| `PADI_ProcessBatch` | 9.56 | 9.15 | 1.0 | 364184 | 10.6846 | 10.2216 |
| `PADI_ClassifyPaths` | 1.72 | 1.09 | 1.0 | 49775 | 1.1639 | 0.7357 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.scenarios[0].samples[2].Last Hit Frame` | 2.0000 | 124.0000 | 122.0000 | 6100.00% | 负收益 |
| `$.scenarios[0].samples[2].Hit Frames` | 1.0000 | 46.0000 | 45.0000 | 4500.00% | 负收益 |
| `$.scenarios[0].samples[2].First Hit Frame` | 2.0000 | 79.0000 | 77.0000 | 3850.00% | 负收益 |
| `$.scenarios[0].samples[2].Hit Frame Ratio` | 1.9600 | 34.5900 | 32.6300 | 1664.80% | 负收益 |
| `$.scenarios[0].samples[2].Self` | 2.0200 | 9.1500 | 7.1300 | 352.97% | 负收益 |
| `$.scenarios[0].samples[2].Total` | 2.2000 | 9.5600 | 7.3600 | 334.55% | 负收益 |
| `$.lastFrameIndex` | 50.0000 | 132.0000 | 82.0000 | 164.00% | 负收益 |
| `$.totalFrames` | 51.0000 | 133.0000 | 82.0000 | 160.78% | 负收益 |
| `$.scenarios[0].measuredFrames` | 51.0000 | 133.0000 | 82.0000 | 160.78% | 负收益 |
| `$.scenarios[0].samples[0].First Hit Frame` | 48.0000 | 124.0000 | 76.0000 | 158.33% | 负收益 |
| `$.scenarios[0].samples[0].Last Hit Frame` | 48.0000 | 124.0000 | 76.0000 | 158.33% | 负收益 |
| `$.scenarios[0].samples[3].Self` | 45.1900 | 1.0900 | -44.1000 | -97.59% | 正收益 |
| `$.scenarios[0].samples[3].Total` | 47.2100 | 1.7200 | -45.4900 | -96.36% | 正收益 |
| `$.scenarios[0].samples[3].Peak Self ms Per Frame` | 19.9050 | 1.0195 | -18.8855 | -94.88% | 正收益 |
| `$.scenarios[0].samples[3].Peak GC Alloc Per Frame` | 1677242.0000 | 87790.0000 | -1589452.0000 | -94.77% | 正收益 |
| `$.scenarios[0].samples[1].Total` | 7.5800 | 0.4400 | -7.1400 | -94.20% | 正收益 |
| `$.scenarios[0].samples[3].Self ms` | 10.3432 | 0.7357 | -9.6075 | -92.89% | 正收益 |
| `$.scenarios[0].samples[3].Peak Time ms Per Frame` | 20.0872 | 1.6279 | -18.4593 | -91.90% | 正收益 |
| `$.scenarios[0].samples[1].Self` | 4.8000 | 0.4000 | -4.4000 | -91.67% | 正收益 |
| `$.scenarios[0].samples[3].Time ms` | 10.8061 | 1.1639 | -9.6422 | -89.23% | 正收益 |
| `$.scenarios[0].samples[2].GC Alloc` | 3136970.0000 | 364184.0000 | -2772786.0000 | -88.39% | 正收益 |
| `$.scenarios[0].samples[3].GC Alloc` | 364184.0000 | 49775.0000 | -314409.0000 | -86.33% | 正收益 |
| `$.scenarios[0].samples[0].Total` | 9.7400 | 2.1100 | -7.6300 | -78.34% | 正收益 |
| `$.scenarios[0].samples[0].Self` | 9.5300 | 2.0700 | -7.4600 | -78.28% | 正收益 |
| `$.scenarios[0].samples[1].Time ms` | 79.8667 | 22.4199 | -57.4468 | -71.93% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 79.8667 | 22.4199 | -57.4468 | -71.93% | 正收益 |
| `$.scenarios[0].samples[3].Hit Frames` | 46.0000 | 76.0000 | 30.0000 | 65.22% | 负收益 |
| `$.scenarios[0].samples[3].Last Hit Frame` | 48.0000 | 78.0000 | 30.0000 | 62.50% | 负收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 1.9600 | 0.7500 | -1.2100 | -61.73% | 正收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 1.9600 | 0.7500 | -1.2100 | -61.73% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 50.5725 | 20.5635 | -30.0090 | -59.34% | 正收益 |
| `$.scenarios[0].samples[1].Self ms` | 50.5725 | 20.5635 | -30.0090 | -59.34% | 正收益 |
| `$.scenarios[0].samples[2].Time ms` | 23.1742 | 10.6846 | -12.4896 | -53.89% | 正收益 |
| `$.scenarios[0].samples[2].Self ms` | 21.2769 | 10.2216 | -11.0553 | -51.96% | 正收益 |
| `$.scenarios[0].samples[2].Peak GC Alloc Per Frame` | 3136970.0000 | 1675048.0000 | -1461922.0000 | -46.60% | 正收益 |
| `$.scenarios[0].samples[3].Hit Frame Ratio` | 90.2000 | 57.1400 | -33.0600 | -36.65% | 正收益 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 3782956.0000 | 3136970.0000 | -645986.0000 | -17.08% | 正收益 |
| `$.scenarios[0].samples[1].GC Alloc` | 3782956.0000 | 3136970.0000 | -645986.0000 | -17.08% | 正收益 |
| `$.scenarios[0].samples[2].Peak Time ms Per Frame` | 23.1742 | 20.0723 | -3.1019 | -13.39% | 正收益 |
| `$.scenarios[0].samples[2].Peak Self ms Per Frame` | 21.2769 | 19.8798 | -1.3971 | -6.57% | 正收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 100.3751 | 106.3218 | 5.9467 | 5.92% | 负收益 |
| `$.scenarios[0].samples[0].Self ms` | 100.3751 | 106.3218 | 5.9467 | 5.92% | 负收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 102.5378 | 108.4149 | 5.8771 | 5.73% | 负收益 |
| `$.scenarios[0].samples[0].Time ms` | 102.5378 | 108.4149 | 5.8771 | 5.73% | 负收益 |
| `$.scenarios[0].samples[0].GC Alloc` | 1553206.0000 | 1552918.0000 | -288.0000 | -0.02% | 持平 |
| `$.scenarios[0].samples[0].Peak GC Alloc Per Frame` | 1553206.0000 | 1552918.0000 | -288.0000 | -0.02% | 持平 |

### 收益判定

**结果：正收益** — 均值指标（Time ms / GC Alloc）改善居多，可保留代码修改。

---

## 2026-09-24 08:00:06 slice-classify-incremental

- 图纸: `ProjectAssetData 单资源增量索引重建` (project-asset-index-incremental-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `11`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 0.36 | 0.15 | 1.0 | 8560 | 1.5989 | 0.6726 |
| `PADI_ProcessBatch` | 0.22 | 0.06 | 1.0 | 2092 | 0.9954 | 0.2462 |
| `PADI_IncrementalScan` | 0.16 | 0.03 | 1.0 | 1338 | 0.7096 | 0.1479 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.lastFrameIndex` | 3.0000 | 10.0000 | 7.0000 | 233.33% | 负收益 |
| `$.scenarios[0].samples[1].Last Hit Frame` | 1.0000 | 3.0000 | 2.0000 | 200.00% | 负收益 |
| `$.scenarios[0].samples[1].First Hit Frame` | 1.0000 | 3.0000 | 2.0000 | 200.00% | 负收益 |
| `$.totalFrames` | 4.0000 | 11.0000 | 7.0000 | 175.00% | 负收益 |
| `$.scenarios[0].measuredFrames` | 4.0000 | 11.0000 | 7.0000 | 175.00% | 负收益 |
| `$.scenarios[0].samples[2].Self` | 0.2400 | 0.0300 | -0.2100 | -87.50% | 正收益 |
| `$.scenarios[0].samples[0].Self` | 0.7900 | 0.1500 | -0.6400 | -81.01% | 正收益 |
| `$.scenarios[0].samples[0].Total` | 1.4800 | 0.3600 | -1.1200 | -75.68% | 正收益 |
| `$.scenarios[0].samples[1].Total` | 0.8500 | 0.2200 | -0.6300 | -74.12% | 正收益 |
| `$.scenarios[0].samples[1].Self` | 0.2300 | 0.0600 | -0.1700 | -73.91% | 正收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 25.0000 | 9.0900 | -15.9100 | -63.64% | 正收益 |
| `$.scenarios[0].samples[2].Hit Frame Ratio` | 25.0000 | 9.0900 | -15.9100 | -63.64% | 正收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 25.0000 | 9.0900 | -15.9100 | -63.64% | 正收益 |
| `$.scenarios[0].samples[2].Total` | 0.4400 | 0.1600 | -0.2800 | -63.64% | 正收益 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 1338.0000 | 2092.0000 | 754.0000 | 56.35% | 负收益 |
| `$.scenarios[0].samples[1].GC Alloc` | 1338.0000 | 2092.0000 | 754.0000 | 56.35% | 负收益 |
| `$.scenarios[0].samples[2].Self ms` | 0.2961 | 0.1479 | -0.1482 | -50.05% | 正收益 |
| `$.scenarios[0].samples[2].Peak Self ms Per Frame` | 0.2961 | 0.1479 | -0.1482 | -50.05% | 正收益 |
| `$.scenarios[0].samples[0].Last Hit Frame` | 2.0000 | 3.0000 | 1.0000 | 50.00% | 负收益 |
| `$.scenarios[0].samples[0].First Hit Frame` | 2.0000 | 3.0000 | 1.0000 | 50.00% | 负收益 |
| `$.scenarios[0].samples[2].Peak Time ms Per Frame` | 0.5372 | 0.7096 | 0.1724 | 32.09% | 负收益 |
| `$.scenarios[0].samples[2].Time ms` | 0.5372 | 0.7096 | 0.1724 | 32.09% | 负收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 0.9631 | 0.6726 | -0.2905 | -30.16% | 正收益 |
| `$.scenarios[0].samples[0].Self ms` | 0.9631 | 0.6726 | -0.2905 | -30.16% | 正收益 |
| `$.scenarios[0].samples[2].GC Alloc` | 1792.0000 | 1338.0000 | -454.0000 | -25.33% | 正收益 |
| `$.scenarios[0].samples[2].Peak GC Alloc Per Frame` | 1792.0000 | 1338.0000 | -454.0000 | -25.33% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 0.2792 | 0.2462 | -0.0330 | -11.82% | 正收益 |
| `$.scenarios[0].samples[1].Self ms` | 0.2792 | 0.2462 | -0.0330 | -11.82% | 正收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 1.8089 | 1.5989 | -0.2100 | -11.61% | 正收益 |
| `$.scenarios[0].samples[0].Time ms` | 1.8089 | 1.5989 | -0.2100 | -11.61% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 1.0461 | 0.9954 | -0.0507 | -4.85% | 正收益 |
| `$.scenarios[0].samples[1].Time ms` | 1.0461 | 0.9954 | -0.0507 | -4.85% | 正收益 |

### 收益判定

**结果：正收益** — 均值指标（Time ms / GC Alloc）改善居多，可保留代码修改。

---

## 2026-09-24 10:46:42 opt-verify-full-clean

- 图纸: `ProjectAssetData 全量索引重建` (project-asset-index-full-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `91`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_SaveSnapshot` | 17.20 | 13.73 | 1.0 | 1448934 | 92.0412 | 73.4369 |
| `PADI_FindAssets` | 2.93 | 2.63 | 1.0 | 1994298 | 15.6733 | 14.0881 |
| `PADI_ProcessBatch` | 36.72 | 33.32 | 1.0 | 388917 | 5.9524 | 5.4011 |
| `PADI_ClassifyPaths` | 9.11 | 5.44 | 1.0 | 47154 | 0.8858 | 0.5287 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.scenarios[0].samples[2].First Hit Frame` | 2.0000 | 58.0000 | 56.0000 | 2800.00% | 负收益 |
| `$.scenarios[0].samples[2].Last Hit Frame` | 34.0000 | 90.0000 | 56.0000 | 164.71% | 负收益 |
| `$.scenarios[0].samples[0].First Hit Frame` | 34.0000 | 90.0000 | 56.0000 | 164.71% | 负收益 |
| `$.scenarios[0].samples[0].Last Hit Frame` | 34.0000 | 90.0000 | 56.0000 | 164.71% | 负收益 |
| `$.lastFrameIndex` | 42.0000 | 90.0000 | 48.0000 | 114.29% | 负收益 |
| `$.totalFrames` | 43.0000 | 91.0000 | 48.0000 | 111.63% | 负收益 |
| `$.scenarios[0].measuredFrames` | 43.0000 | 91.0000 | 48.0000 | 111.63% | 负收益 |
| `$.scenarios[0].samples[2].Self` | 15.9800 | 33.3200 | 17.3400 | 108.51% | 负收益 |
| `$.scenarios[0].samples[2].Total` | 17.7000 | 36.7200 | 19.0200 | 107.46% | 负收益 |
| `$.scenarios[0].samples[1].Last Hit Frame` | 1.0000 | 2.0000 | 1.0000 | 100.00% | 负收益 |
| `$.scenarios[0].samples[1].First Hit Frame` | 1.0000 | 2.0000 | 1.0000 | 100.00% | 负收益 |
| `$.scenarios[0].samples[0].GC Alloc` | 11125529.0000 | 1448934.0000 | -9676595.0000 | -86.98% | 正收益 |
| `$.scenarios[0].samples[0].Peak GC Alloc Per Frame` | 11125529.0000 | 1448934.0000 | -9676595.0000 | -86.98% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 94.5053 | 15.6733 | -78.8320 | -83.42% | 正收益 |
| `$.scenarios[0].samples[1].Time ms` | 94.5053 | 15.6733 | -78.8320 | -83.42% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 61.1207 | 14.0881 | -47.0326 | -76.95% | 正收益 |
| `$.scenarios[0].samples[1].Self ms` | 61.1207 | 14.0881 | -47.0326 | -76.95% | 正收益 |
| `$.scenarios[0].samples[1].Total` | 10.0600 | 2.9300 | -7.1300 | -70.87% | 正收益 |
| `$.scenarios[0].samples[0].Total` | 10.2500 | 17.2000 | 6.9500 | 67.80% | 负收益 |
| `$.scenarios[0].samples[1].Self` | 6.5100 | 2.6300 | -3.8800 | -59.60% | 正收益 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 4783918.0000 | 1994298.0000 | -2789620.0000 | -58.31% | 正收益 |
| `$.scenarios[0].samples[1].GC Alloc` | 4783918.0000 | 1994298.0000 | -2789620.0000 | -58.31% | 正收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 2.3300 | 1.1000 | -1.2300 | -52.79% | 正收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 2.3300 | 1.1000 | -1.2300 | -52.79% | 正收益 |
| `$.scenarios[0].samples[2].Hit Frame Ratio` | 76.7400 | 36.2600 | -40.4800 | -52.75% | 正收益 |
| `$.scenarios[0].samples[2].Peak Self ms Per Frame` | 12.6767 | 18.6914 | 6.0147 | 47.45% | 负收益 |
| `$.scenarios[0].samples[2].Peak Time ms Per Frame` | 14.6217 | 20.1499 | 5.5282 | 37.81% | 负收益 |
| `$.scenarios[0].samples[0].Self` | 10.0200 | 13.7300 | 3.7100 | 37.03% | 负收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 94.1340 | 73.4369 | -20.6971 | -21.99% | 正收益 |
| `$.scenarios[0].samples[0].Self ms` | 94.1340 | 73.4369 | -20.6971 | -21.99% | 正收益 |
| `$.scenarios[0].samples[2].Self ms` | 4.5482 | 5.4011 | 0.8529 | 18.75% | 负收益 |
| `$.scenarios[0].samples[2].Time ms` | 5.0365 | 5.9524 | 0.9159 | 18.19% | 负收益 |
| `$.scenarios[0].samples[2].Peak GC Alloc Per Frame` | 1790784.0000 | 1572354.0000 | -218430.0000 | -12.20% | 正收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 96.2834 | 92.0412 | -4.2422 | -4.41% | 正收益 |
| `$.scenarios[0].samples[0].Time ms` | 96.2834 | 92.0412 | -4.2422 | -4.41% | 正收益 |

### 收益判定

**结果：正收益** — 均值指标（Time ms / GC Alloc）改善居多，可保留代码修改。

---

## 2026-09-24 10:50:57 opt-verify-incremental

- 图纸: `ProjectAssetData 单资源增量索引重建` (project-asset-index-incremental-rebuild)
- 场景: ``
- 目标: ``
- 组件: `ProjectAssetIndexProfilerTool`
- 策略: `editor_window_profiler_recording`
- 录制时长: `10.0s`

### Profiler Columns

#### 全帧汇总

- 帧数: `6`

| Marker | Total | Self | Calls | GC Alloc | Time ms | Self ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| `PADI_ProcessBatch` | 2.47 | 0.13 | 1.0 | 1792 | 5.5299 | 0.2810 |
| `PADI_SaveSnapshot` | 0.47 | 0.25 | 1.0 | 8560 | 1.0407 | 0.5686 |
| `PADI_IncrementalScan` | 0.31 | 0.08 | 1.0 | 1338 | 0.7023 | 0.1720 |

### Baseline 对比

| 路径 | Baseline | 当前 | Delta | Delta % | 趋势 |
| --- | ---: | ---: | ---: | ---: | --- |
| `$.scenarios[0].samples[1].Last Hit Frame` | 1.0000 | 3.0000 | 2.0000 | 200.00% | 负收益 |
| `$.scenarios[0].samples[1].First Hit Frame` | 1.0000 | 3.0000 | 2.0000 | 200.00% | 负收益 |
| `$.lastFrameIndex` | 2.0000 | 5.0000 | 3.0000 | 150.00% | 负收益 |
| `$.totalFrames` | 3.0000 | 6.0000 | 3.0000 | 100.00% | 负收益 |
| `$.scenarios[0].measuredFrames` | 3.0000 | 6.0000 | 3.0000 | 100.00% | 负收益 |
| `$.scenarios[0].samples[1].Peak GC Alloc Per Frame` | 13373724.0000 | 8560.0000 | -13365164.0000 | -99.94% | 正收益 |
| `$.scenarios[0].samples[1].GC Alloc` | 13373724.0000 | 8560.0000 | -13365164.0000 | -99.94% | 正收益 |
| `$.scenarios[0].samples[0].Self ms` | 333.7629 | 0.2810 | -333.4819 | -99.92% | 正收益 |
| `$.scenarios[0].samples[0].Peak Self ms Per Frame` | 333.7629 | 0.2810 | -333.4819 | -99.92% | 正收益 |
| `$.scenarios[0].samples[0].Peak GC Alloc Per Frame` | 994258.0000 | 1792.0000 | -992466.0000 | -99.82% | 正收益 |
| `$.scenarios[0].samples[0].GC Alloc` | 994258.0000 | 1792.0000 | -992466.0000 | -99.82% | 正收益 |
| `$.scenarios[0].samples[1].Self ms` | 222.3194 | 0.5686 | -221.7508 | -99.74% | 正收益 |
| `$.scenarios[0].samples[1].Peak Self ms Per Frame` | 222.3194 | 0.5686 | -221.7508 | -99.74% | 正收益 |
| `$.scenarios[0].samples[0].Self` | 50.7400 | 0.1300 | -50.6100 | -99.74% | 正收益 |
| `$.scenarios[0].samples[1].Time ms` | 268.2877 | 1.0407 | -267.2470 | -99.61% | 正收益 |
| `$.scenarios[0].samples[1].Peak Time ms Per Frame` | 268.2877 | 1.0407 | -267.2470 | -99.61% | 正收益 |
| `$.scenarios[0].samples[1].Self` | 33.8000 | 0.2500 | -33.5500 | -99.26% | 正收益 |
| `$.scenarios[0].samples[1].Total` | 40.7900 | 0.4700 | -40.3200 | -98.85% | 正收益 |
| `$.scenarios[0].samples[0].Peak Time ms Per Frame` | 334.5934 | 5.5299 | -329.0635 | -98.35% | 正收益 |
| `$.scenarios[0].samples[0].Time ms` | 334.5934 | 5.5299 | -329.0635 | -98.35% | 正收益 |
| `$.scenarios[0].samples[0].Total` | 50.8700 | 2.4700 | -48.4000 | -95.14% | 正收益 |
| `$.scenarios[0].samples[2].Peak Time ms Per Frame` | 5.5462 | 0.7023 | -4.8439 | -87.34% | 正收益 |
| `$.scenarios[0].samples[2].Time ms` | 5.5462 | 0.7023 | -4.8439 | -87.34% | 正收益 |
| `$.scenarios[0].samples[2].Total` | 0.8400 | 0.3100 | -0.5300 | -63.10% | 正收益 |
| `$.scenarios[0].samples[2].Self ms` | 0.3665 | 0.1720 | -0.1945 | -53.07% | 正收益 |
| `$.scenarios[0].samples[2].Peak Self ms Per Frame` | 0.3665 | 0.1720 | -0.1945 | -53.07% | 正收益 |
| `$.scenarios[0].samples[0].Last Hit Frame` | 2.0000 | 3.0000 | 1.0000 | 50.00% | 负收益 |
| `$.scenarios[0].samples[0].First Hit Frame` | 2.0000 | 3.0000 | 1.0000 | 50.00% | 负收益 |
| `$.scenarios[0].samples[1].Hit Frame Ratio` | 33.3300 | 16.6700 | -16.6600 | -49.98% | 正收益 |
| `$.scenarios[0].samples[0].Hit Frame Ratio` | 33.3300 | 16.6700 | -16.6600 | -49.98% | 正收益 |
| `$.scenarios[0].samples[2].Hit Frame Ratio` | 33.3300 | 16.6700 | -16.6600 | -49.98% | 正收益 |
| `$.scenarios[0].samples[2].Self` | 0.0600 | 0.0800 | 0.0200 | 33.33% | 负收益 |
| `$.scenarios[0].samples[2].GC Alloc` | 1920.0000 | 1338.0000 | -582.0000 | -30.31% | 正收益 |
| `$.scenarios[0].samples[2].Peak GC Alloc Per Frame` | 1920.0000 | 1338.0000 | -582.0000 | -30.31% | 正收益 |

### 收益判定

**结果：正收益** — 均值指标（Time ms / GC Alloc）改善居多，可保留代码修改。

---
