## About / Synopsis

* Demos and tools developed for the ARETE project

Some of the tools developed here rely on Orkestra to function (specifically, its [Unity implementation](https://github.com/tv-vicomtech/orkestralib-unity)), a library for multi-user communication, so be sure to include the necessary dependencies when deploying the code.

## Table of contents

  * [About / Synopsis](#about--synopsis)
  * [Table of contents](#table-of-contents)
  * [Demos](#demos)
  * [Usage](#usage)
  * [Resources (Documentation and other links)](#resources-documentation-and-other-links)

## Demos
- [Handshake Demo](https://github.com/tv-vicomtech/ARETE/tree/AnimationDemos/Unity3D/HandShake)
- [Stand Up For Other](https://github.com/tv-vicomtech/ARETE/tree/AnimationDemos/Unity3D/Bullying)
- [Organize Space](https://github.com/tv-vicomtech/ARETE/tree/AnimationDemos/Unity3D/OrganizeSpace)

## Requirements
- Android 7.0 (API level 24) or higher
- iOS 11 or higher

## Usage

Clone the project from the repository: 
```
  git clone  https://username@github.com/tv-vicomtech/ARETE.git
```

Download the latest version of the OrkestraLib and the Socket.IO package from the OrkestraLib-Unity [releases](https://github.com/tv-vicomtech/orkestralib-unity/releases/).
![image](https://user-images.githubusercontent.com/25354672/142851392-965922d2-f31a-421d-be7e-7343407199c4.png)

Open with Unity the ARETE/Unity3D folder.

Open the OrkestraLib.unitypackage and the socketIO package.

![image](https://user-images.githubusercontent.com/25354672/142626321-3ee12ed1-83ee-404b-b7b4-bfcc0c3af402.png)

Everything is ready!

For more information about how to use Orkestra from within a Unity project, please check the README inside the Unity3D folder ([Basic demo](https://github.com/tv-vicomtech/ARETE/tree/develop/Unity3D/SimpleDemo) and [Geography demo](https://github.com/tv-vicomtech/ARETE/tree/develop/Unity3D/PlanetDemo))

## Resources (Documentation and other links)

[OrkestraLib](https://github.com/tv-vicomtech/orkestralib-unity/releases/) - OrkestraLib releases. 

### Legacy
Some of the old files and projects developed for the project have been moved to Vicomtech's Box (`ARETE_EU1367_2018 > 3.Informacion_durante_el_proyecto > Port Unity Orkestralib`). The files moved to Box contain the following folders:

      The folder [TFM_Anne-Claire](TFM_Anne-Claire) contains the code developed by Anne-Claire Fouchier for markerless AR using computer vision models

      The folder [TFG_Alvaro](TFG_Alvaro) contains the doe developed by Alvaro Cabrero for AR for multi-user musical effects

      The folder [XAPI](XAPI) contains the code used for the [Learning Locker](https://learninglocker.vicomtech.org) server installed at Vicomtech, as well as the code for the automatic generation of xAPI related code for Unity and web applications

      The folder [Twitter_Analytics](Twitter_Analytics) contains some R code use to analyze the Twitter traffic related to the ARETE project. 
