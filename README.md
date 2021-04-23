[TOC]

## 概述
项目名: OdyHostNginx

gitlab仓库地址: http://git.odianyun.com/luozhuowei/OdyHostNginx

软件安装下载地址: https://veasion-oss.oss-cn-shanghai.aliyuncs.com/soft/OdyHostNginx.rar

项目简介：
Ody项目定制化开发工具（目前只支持Windows），快速创建项目环境配置，通过按钮开关就可以把线上请求转发到本地debug调试，支持nginx转发、host、http抓包、修改请求响应、查看trace、查看pool日志等一系列功能。

解决一下痛点：
1.二开项目需要切换不能的pool开发，需要nginx和host映射到本地开发和调试，nginx配置太繁琐
2.抓包和修改请求响应太麻烦，不是项目上的包一大堆，还有https的，不支持直接查看请求 traces
3.traces 查看，网页版不灵活
4.开发使用的辅助工具太多，如 nginx、switch host、fiddler、charles




## 1. 如何运行

- 下载软件压缩包:  https://veasion-oss.oss-cn-shanghai.aliyuncs.com/soft/OdyHostNginx.rar

- 解压到没有中文的路径目录（目前权限必须修改权限）

- 双击 OdyHostNginx.exe 运行 （如有请求权限网络弹框请一律点击允许）

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx01.png)



## 2. 环境配置

运行后需要配置环境，比如新增 296trunk 环境

具体步骤：

- 点击 Edit > Add Ody Env

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/addEnv01.png)

- 填写 Env 环境名称（数字字母下划线），然后填写需要代理的 Domain 域名（后台域名/前台域名），填写完域名后Host Ip 会自动获取（域名对应的IP地址）。

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/addEnv02.png)

- 点击 OK 新增环境完成

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx02.png)



因为296trunk需要配置开发 host，所以这里需要新增一个环境 Host 分组。

- 点击 Edit > Add Host Group

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/addHost01.png)

- Host分组命名（数字字母下划线），这里跟环境名字一样，容易区分

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/addHost02.png)

- host分组新增完成，需要添加 host，选中host，然后点击 + 号

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/addHost03.png)

- 复制wiki环境上的host，粘贴到这个里面，点击 confirm 确认

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/addHost04.png)

- host分组新增完成，整个296trunk环境就弄好了。

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx04.png)



## 3.指定pool代理到本地debug

- 选择环境，点击搜索按钮，输入 pool 名称搜索（这里以 oms-web 为例）

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx05.png)

- 点击 pool 右边的 local 按钮，代理到本地


![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx06.png)

- 修改端口，端口为本地启动应用的端口 application.yml 中 server.port 没有就默认填 8080，然后点击 √ 应用


![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx07.png)

- 打开总开关，环境开关，host 开关，这是就已经开启代理了，如果想停掉直接关闭总开关就行。


![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/odyHostNginx08.png)



## 4.HTTP抓包、查看trace、修改请求响应

- 点击 Tools > Http Packet 打开抓包工具

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/http01.png)
- 访问网站，抓包 （这里只抓XHR请求），选择请求查看请求 Info 信息

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/http02.png)
- 选择右边卡片 json 查看响应，格式化后的 json 数据

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/http03.png)
- 选择右边卡片 Trace 查看请求的 trace 链路，选中select (db) 可查看执行了哪些 sql

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/http04.png)
- 选中请求，鼠标右键可以复制请求url、重新请求、保存请求响应、修改请求、修改响应、修改全部

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/http05.png)
- 右键 Modify Response 修改请求响应数据，修改完成后点击 apply 应用，下次请求会自动修改。

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/http06.png)



## 5.非docker服务新增环境

- 不是在一台服务器的非docker环境，可以通过 File > Import 把环境上的nginx配置导入进来，导入后会自动生成环境信息。

![image](https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/image/import.png)

