# OpenMomordica 开源苦瓜

Momordica VDM - coolQ bot version

虚拟人形苦瓜 - 酷Q机器人版

## 基本介绍

- 本程序是一个酷Q插件，提供多种qq聊天自动回复功能

## 具备功能

- 语料随机拼接回复
- roll dice 掷骰子
- 淫梦民最喜欢的全自动数字论证
- ~~百度知识图谱信息检索~~（受百度反爬虫限制，不太好用）
- 百度百科/百度贴吧内容检索
- ~~国内天气查询~~（暂不再支持）
- 赛🐎小游戏
- 图片转字符画
- 特殊符号文本生成
- ~~藏头诗、藏尾诗生成~~（语料库比较大，暂未同步该模块所需数据）
- 多语种翻译（基于google translate）
- 抽卡
- 周易占卜（赛博蓍草法起卦）
- ……

## 运行步骤

1. 安装本插件的运行依赖库 `.net framework 4.5+` 和 `coolQ Air`
2. 编译本插件源码，生成 `app.dll` `app.json` 两个文件，并将这两个文件放置于酷Q程序根目录下 `dev/me.cqp.hontsev.demo/` 文件夹内，使酷Q启动时可以自动加载本插件
3. 将本插件提供的默认资源文件 `RunningData` 内的文件全部拷贝至酷Q程序根目录下 `data/app/me.cqp.hontsev.demo/` 文件夹内，使本插件在初始化时可读取资源文件
4. 修改酷Q程序根目录下 `data/app/me.cqp.hontsev.demo/config.txt` 这一配置文件内的配置项，为本插件设置必要的运行数据。根据需要，也可以修改其他资源文件内容
5. 启动酷q，登录插件要挂载的qq账号，并在酷q的插件管理界面启动本插件
6. 由其他qq账号私聊或在群组内与挂载本插件的qq账号互动，即可使用本插件提供的功能

## 开发须知

- 本程序基于 [基于C#的酷Q二次开发框架CQP](https://github.com/Flexlive/CQP/) 开发，部署、调试流程可参考 [该框架的说明](https://cqp.cc/t/29261)
- 业务处理相关代码主要在 `Native.Csharp.Frame-Final\Native.Csharp.Frame-Final\Native.Csharp\App` 路径下
- 语料库资源及程序运行时配置要放置在酷Q插件的对应路径下，即酷Q运行路径下的 `data\app\me.cqp.hontsev.demo\` 里。具体文件类型和数据格式暂无说明，请参考代码中的读取过程。
- 建议使用Visual Studio 2015 或更高版本进行开发（需要支持C# 6.0语法糖），否则可能会编译报错。






