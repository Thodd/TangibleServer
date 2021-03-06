TangibleServer
==============

A Tangible-Websocket Bridge to access Pixelsense-Tags from JavaScript.

Microsoft's Surface SDK 2.0 for their Pixelsense (SUR40) device, offers the capability to access `Tags`.

Tags are, simply put, proprietary optical markers, used to track physical objects on the Pixelsense Multi-Touch-Table (via infra-red cameras).

This project implements a simple Websockets interface in order to transmit the native Windows Touch-/Tag-Events to the browser.

An example on how to implement the JavaScript part, can be found in the `site` folder.

![alt tag](https://raw.github.com/Thodd/TangibleServer/master/tag2js.png)

### Dependencies
Alchemy Websockets for a simple WS Server on the C# side.

http://alchemywebsockets.net/
