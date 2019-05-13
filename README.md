# Snail.ImageThumb
A image thumb maker write by csharp

基于.Net Framework 4.0。

这是一个控制台程序，使用它可以为指定的目录（可以配置多个），按照指定的分辨率生成缩略图。

## 关于配置文件

```xml

<?xml version="1.0" encoding="utf-8" ?>
<config>
  <paths>
    <path>
      <input>E:\Workspace\Snail.ImageThumb\test\input</input>
      <output>E:\Workspace\Snail.ImageThumb\test\savedir</output>
      <size>320x0</size>
    </path>
    <path>
      <input>E:\Workspace\Snail.ImageThumb\test\input2</input>
      <output>E:\Workspace\Snail.ImageThumb\test\savedir2</output>
      <size>120x240</size>
    </path>
  </paths>
</config>

```

其中，`size`格式为WidthxHeight，Width或Height为0则表示保持原图比例。
