# OpenMomordica 开源苦瓜

Momordica VDM - coolQ bot version

虚拟人形苦瓜 - 酷Q机器人版

## 基本介绍

- 本程序是一个酷Q插件，提供多种qq聊天自动回复功能

## 具备功能

- 语料随机拼接回复
- roll dice 掷骰子
- 淫梦民最喜欢的数字论证
- 百度知识图谱信息检索
- 百度百科/百度贴吧内容检索
- 国内天气查询
- 多语种翻译

## 使用须知

- 本程序的运行依赖 `.net framework 4.5+` `coolQ Air` 请务必安装
- 将本程序生成的包含 `app.dll` `app.json` 放置于酷q（coolQ）程序根目录下的 `./dev/me.cqp.hontsev.demo/` 中，使酷Q启动时可以自动加载本插件
- 启动酷q，登录要挂载的qq账号，并在酷q的插件管理界面启动本插件

## 开发须知

- 本程序基于 [基于C#的酷Q二次开发框架CQP](https://github.com/Flexlive/CQP/) 开发，部署、调试流程可参考 [该框架的说明](https://cqp.cc/t/29261)
- 业务处理相关代码主要在 `Native.Csharp.Frame-Final\Native.Csharp.Frame-Final\Native.Csharp\App` 路径下
- 语料库资源及程序运行时配置要放置在酷Q插件的对应路径下，即酷Q运行路径下的 `data\app\me.cqp.hontsev.demo\` 里。具体有哪些文件参考代码中的读取过程。






