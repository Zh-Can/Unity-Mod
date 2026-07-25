# Core

Core 是 Mod 的基础核心模块。

负责提供与具体游戏无关的基础能力，包括：

- 日志系统:
  - ILogger.cs, Log.cs, UnityDebugLogger.cs
  - 日志适配器：BepinEXLogger.cs, MelonLoggerAdapter.cs
- 配置管理:
  - ModConfig.cs,基本配置，保存注册表的，目前存`缩放比`和`语言`
- Mod 信息管理


Core 不应该包含任何游戏逻辑。
