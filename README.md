## 2022.07.03 ##
- 新增web版聊天窗口（Windows客户端不支持）
- 新增web版隐藏动画（Windows客户端不支持）
- 补充web版录像回放功能

## 2022.06.23 ##
- 新增web版特效（Windows客户端不支持）
- 补充web版切牌功能
- 补充web版语音链接设置功能
- 修复bug：对2被视为拖拉机
- 修复bug：切牌顺序错误

## 2022.06.18 ##
- 新增web版弹幕功能（Windows客户端只能接收弹幕，不能发送弹幕）

## 2022.06.10 ##
- 新增就绪音效
- 恢复右键显示亮牌信息
- 最后一圈跟出玩家若离线则自动出
- 修复web版初始无音效的bug
- 修复web版房间内玩家状态信息不更新的bug
- 修复web版上家出完牌后下家第一次点牌没反应的bug

## 2022.06.08 ##
- 修复web版离线导致游戏重开的bug
- 修复继续牌局成功后打几、主牌、庄家信息未正确显示的bug
- Web版功能：
    - 出牌音效
    - 大牌、杀牌标记
    - 上轮回看
    - 游戏设置、操作指南菜单
    - 继续上盘牌局
    - 自动加载上次输入的服务器、昵称信息
    - 修复提前选牌后不出现确定按钮的bug
    - 拖拽选牌
    - 右键智能选牌
    - 修复最后一把不自动跟出的bug
    - 优化亮牌图标
    - 常用快捷键
    - 摸底埋底动画

## 2022.06.05 ##
- 新增web版客户端功能

## 2022.05.12 ##
- 恢复换座功能

## 2022.03.26 ##
- bug修复：当自动选牌时打出的牌顺序紊乱

## 2022.02.23 ##
- 优化右键选牌：领出玩家如果右键点的A或者大王，且满足甩多张的条件，则向左选中所有该花色中合理可甩的牌

## 2022.01.17 ##
- 新增表情包烟花效果，并在游戏结束时播放
- 修改甩牌检测机制为非同步，解决因甩牌导致的掉线问题
- 强制此程序为 Single Instance Application

## 2022.01.12 ##
- 给发送表情包增加快捷键

## 2022.01.03 ##
- 优化：在大厅中也显示旁观玩家

## 2022.01.02 ##
- 修复bug：看不见旁观玩家
- UI微调

## 2022.01.01 ##
- 新增功能：表情包

## 2021.12.31 ##
- 改进大牌和调主图标

## 2021.12.29 ##
- bug修复：如果所有人都就绪了，由于网络延迟导致之前的有一人未就绪的来自服务器的消息滞后到达，导致就绪状态错乱

## 2021.12.19 ##
- 允许取消就绪

## 2021.12.11 ##
- 修复：客户端甩牌时容易crash的bug

## 2021.12.04 ##
- 修复：当用由一对主级牌和一对副级牌组成的拖拉机敲底时，拖拉机不被识别的bug
- 修复：当处理切牌或者投降跳出的窗口时，若玩家操作过慢，则会触发离线检测的bug
- 修复：投降后若游戏意外重置，然后尝试继续上盘游戏时，会回到投降前埋底的状态且投降按钮已移除导致不能正常再次投降的bug，应该从投降后的状态重开游戏即可

## 2021.11.26 ##
- 优化断线检测机制（最多10秒以内检测到玩家断线状态）
- 调整初始界面从而不挡住登录报错信息

## 2021.11.23 ##
- 修复bug：最后一圈如果出现连续盖杀3次（杀、火杀、雷杀）则游戏报错
- 修复bug：拖拉机检测机制错误导致不合理的自动选牌

## 2021.11.16 ##
- 因为某些人令人捉急的网速，加一个实时检测客户端是否离线的机制
- 因为某些人好奇发完牌后屏幕中会有个点，只好扩大了黑板刷
- 一些小bug的修复（游戏重开后等级未重置、还原游戏后读秒仍在）

## 2021.11.07 ##
- 修复bug：在有人离线时旁观玩家退出房间时会触发清理离线玩家及重开游戏
- 缩短发底牌动画的时间

## 2021.11.02 ##
- 录像回放显示甩牌失败的信息
- 游戏中途玩家退出，此局游戏不中断，此玩家显示为离线
- 仅在初次进入房间以及房间设置被更改时才显示房间设置信息

## 2021.10.25 ##
- 修复bug：录像回放界面重复加载下拉菜单的bug
- 清理无用代码：反底
- 修复一些与断线、退出相关的小bug

## 2021.10.11 ##
- 修复bug：加载上盘游戏后，观看上一轮出牌信息缺失
- 修复bug：加载上盘游戏后，庄家底牌未显示
- 修复旁观界面的一些小bug

## 2021.10.10 ##
- 修复自动选牌时选出的拖拉机牌序颠倒的bug
- 取消游戏中途不允许退出的限制，改为退出前提示确认

## 2021.10.03 ##
- 随机算法改为：RNGCryptoServiceProvider
- 录像文件夹下拉菜单优先选择最新的

## 202109.25 ##
- 简化【继续上盘牌局】菜单为一个按钮，并自动选择最合理的还原方式
- bug fix：亮完牌后，出牌前应把突起亮过的牌归位

## 202109.6 ##
- 新增功能：从断点处直接继续上盘游戏
- bug fixes: 离线托管，断线重连，旁观体验
- 优化录像回放界面

## 202109.5 ##
- 规则调整：如果无人亮主，则打无主
- 修复录像回放时分数统计紊乱的bug

## 202109.4 ##
- 优化功能：录像回放支持手动播放上一轮和下一轮
- 优化功能：录像回放支持切换视角

## 202109.3 ##
- 新增功能：录像回放

## 202109.2 ##
- 新增选项：允许关闭切牌提示

## 202109.1 ##
- 修复：加入旁观时若正好轮到被旁观者出牌，则会报错的bug

## 202108 ##
- 领出牌时，右键点散牌则向左选中所有本门花色的牌
- 玩家断线后有5分钟时间断线重连，否则自动托管
- 支持埋底阶段断线重连
- 新增切牌功能
- 新增投降询问队友功能
- 新增房间设置：允许托管自动亮牌

## 20210729 ##
- 得分明细
- 带对子杀散牌甩牌不应算作双敲
- 敲底分数按照2的指数级翻倍
- 去掉托管自动亮牌功能（还原实战体验）

## 20210718 ##
- 增加“加入语音”按钮（链接可在游戏设置中修改）
- 打无主时级牌加小王对子调整为合法拖拉机
- 优化旁观体验
    - 可查看上轮出牌
    - 旁观庄家时底牌可见
    - 场上有人甩牌失败时，失败提示可见

## 20210715 ##
- 实时显示得分牌

## 20210711 ##
- 新增吊主提示标记和音效
- 音效音量可调节
- 放大亮牌图标

## 20210621 ##
- 新增手牌每种花色的张数标记（可设置）

## 20210620 ##
- 修复重大bug：打无主，甩对子加单张，其他玩家只要出相应数量的主对，即使带上副牌也视为毙牌的bug
- 新增音效：抢庄开始前，摸牌时
- 房主可以将玩家请出房间
- 房主退出，房主由位置最靠前的玩家担任

## 20210609 ##
- 跟出玩家自动选择必选牌
- 游戏中途不允许退出
- 游戏中途离线者由电脑托管代打
- 游戏中途离线者可在此盘游戏结束前断线重连

## 20210609 ##
- 新增可设置音效
- 改为每出一张牌都会提示当前最大牌

## 20210604 ##
- 新增可设置功能
    - 必打
    - J到底
    - 可投降
    - 可无分革命
    - 可无主革命

## 20210531 ##
- 新增一常用快捷键（详情见菜单中使用说明）

## 20210531 ##
- 新增一圈牌中的大牌提示
- 开启托管也能触发当前出牌动作
- 将出牌方式由小猪改为按钮
- 将查看上轮出牌、亮牌、得分牌合并至右键触发

## 20210522 ##
- 优化机制：单独显示上轮出牌
- 优化机制：四个王亮无主优先亮大王
- 修复bug：提前选好下一圈出的牌，有时候会失效，还得收回去再重新点
- 优化：游戏消息不再跳出弹窗，而是显示文字
- 新增提示：甩牌失败会有文字提示
- 新增提示：无人亮主庄家自动下台前会有文字提示

## 20210502 ##
- 优化功能：摸牌时开启托管在达到5张时自动亮牌，摸牌结束时自动取消托管
- 新增功能：底牌对庄家一直可见

## 20210313 ##
- 新增功能：允许还原上盘手牌
- 新增功能：支持选座
- 新增：玩家断线保护机制，防止游戏crash

## 20210313 ##
- 新增功能：玩家退出房间
- 新增功能：查看上轮出牌
- 新增功能：查看亮过的牌
- 新增功能：查看得分牌
- 新增功能：最后一轮自动出

## 20210312 ##
- 新增功能：游戏大厅支持多房间

## 20210307 ##
- 新增功能：旁观模式
- 新增功能：简单托管代打模式

## 20210228 ##
- 支持检测更新
- 自由组队
- 随机组队
- 小猪挪右边
- 就绪快捷键F1

## 20210227 ##
- Increase size/font

## 20210226
- 2233为主牌拖拉机被退回
- 去掉双击出牌（容易出错）

## 20210222 ##
- 显示倒计时

## 20210222 ##
- 查看底牌
- 设置从几打起
- 打完A提示赢家，重置开始
- 增加庄家摸底牌的动画
- 修复bug：甩拖拉机带单张，最后一个出牌的不能毙

## 20210221 ##
- 保存读取游戏
- 玩家座位固定
- 右键:
  - 埋牌选所有
  - 第一手选所有对子
  - 跟出，选当前张数

## 20210219 ##
- 5,10,K必打
- 甩错惩罚
- 出牌灯牌边边
- 显示谁按了就绪
- disable最小化托盘
- 对家亮主放中间
- 加个端口号设置
- 超过4人显示房间已满
- 修复有人退出则队友顺序被打乱
- Slower distribute cards speed
- 显示庄家
- UI to configure
- Reset timer after fan zhu
- Show player nick name
- ready button
