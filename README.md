# URP_TemplateShaderForVFXArtist
## 使用须知（必读！）
### 1、此shader仅供特效入门学习，只提供基础的特效功能，并且由于shadergraph本身原因（1.宏的数量有限2.branch节点也只是三元运算符其实都跑了一遍）不方便做优化，此sahder不考虑性能因素只提供效果。后续可能会发布更多有意思的进阶shader。
### 2、需要shadergraph支持shadergraphGUI的版本，最好保证unity2020以上版本，我个人使用的unity版本为2020.2.2f1c1
### 3、所有参数面板的是否粒子系统控制UV ，对应的开启customData XY通道控制UV XYspeed。顶点色RGB通道控制颜色倾向，A通道控制透明度。
### 4、如果对效果有想法，有余力的可以自己改动，各个模块都已经分类完毕打上了组。

 
