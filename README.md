# Pelco VideoXpert Media/Metadata Framework

The Pelco Media/Metadata framework is an application programming framework, written in C#
, for creating metadata servers and consumers. With the main goal of simplifying integration
of Metadata into a VideoXpert system; such as Vx Professional, and/or Vx Enterprise/Ultimate.

It is recommended that you use this framework when creating OpsCenter plugins that consume
metadata as it takes care of most of the protocol and Vx integration challenges.

The framework provides the following functionaliy...

- Create RTSP Clients
- Create RTSP Servers
- Customized processing of Metadata through extensible media processing pipeline.
- Creation of VideoXpert Metadata client (simplifies switching between live and playback).
- Video overlay functionality used for drawing simple shapes on top of video.

## Links
[Documentation](https://github.com/pelcointegrations/Pelco-Media/wiki/Pelco-VideoXpert-Media-Metadata-Framework)

## How To Build

You will require the following to build:
- [Visual Studio 2017](https://github.com/pelcointegrations/Pelco-Media/wiki)

Load the Pelco.Media.sln file into visual studio.  It's that simple.
