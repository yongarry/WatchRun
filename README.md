# WatchRun
**Advanced Digital Fabrication and Practice Final Project**

<p align="center">
<img width="532" alt="image" src="https://github.com/yongarry/WatchRun/assets/71639336/d642c70e-1fcc-498c-b9ab-da895bb4eb56">
</p>

WatchRun is an immersive game designed for smartwatch that combines real-life physical activity with virtual gameplay. By utilizing the built-in pedometer and heart rate monitor, players can synchronize their movements with their in-game avatar, enabling them to run, walk, and even punch through obstacles. The game offers extensive avatar customization,allowing players to create unique characters and share them within a vibrant metaverse. With WatchRun, exercise becomes an interactive and engaging experience, blurring the boundaries between the real and virtual worlds.

## Dependency
**Used Asset**
- [Modular Animated Wood Panel Wall Kit](https://assetstore.unity.com/packages/3d/environments/modular-animated-wood-panel-wall-kit-vertical-wall-destructible--178400)
- [Buttons Set](https://assetstore.unity.com/packages/2d/gui/buttons-set-211824)
- [Realistic Tree 9 [Rainbow Tree]](https://assetstore.unity.com/packages/3d/vegetation/trees/realistic-tree-9-rainbow-tree-54622)
- [Grass Flowers Pack Free](https://assetstore.unity.com/packages/2d/textures-materials/nature/grass-flowers-pack-free-138810)
- [VIS - PBR Grass Textures](https://assetstore.unity.com/packages/2d/textures-materials/floors/vis-pbr-grass-textures-198071)
- [Road Architect](https://github.com/MicroGSD/RoadArchitect)

**Used API**
- [BLE Window API](https://github.com/adabru/BleWinrtDll)

## Hardware
**Used hardware**
- Arduino Nano 33 BLE
- DFRobot heart rate sensor (SEN0203)
- YwRobot OLED display module DIS060010 (Driver IC: SSD1306)
- TP4056: Battery Charge module
- Lithium Polymer Battery

**CAD Design**
<p align="center">
<img width="40%" height="40%" alt="cad" src="https://github.com/yongarry/WatchRun/assets/71639336/d80c0508-53a3-4269-b7c2-df7d5f0589eb">
</p>
STL files will be uploaded.

**Arduino**
- Library Dependency
    - ArduinoBLE.h
    - Arduino_LSM9DS1.h
    - Adafruit_SSD1306.h
    - DFRobot_Heartrate.h
    - Scheduler.h

- Code
```c
#include <ArduinoBLE.h>
#include <Arduino_LSM9DS1.h>
#include <Wire.h>
#include <Adafruit_SSD1306.h>
#include "DFRobot_Heartrate.h"
#include <Scheduler.h>

#define SCREEN_WIDTH 128 // OLED display width, in px
#define SCREEN_HEIGHT 64 // OLED DISPLAY HEIGHT, in px
#define OLED_RESET 0 // setting the reset pin for display (0 or -1 if no reset pin)

#define heartratePin A3

BLEService pedoService("5db62101-443a-4599-83cd-af276d691eab");
BLEStringCharacteristic pedoCount("b9da07a9-85a0-4d4a-8ae2-9d05a973f9ab", BLERead | BLENotify, 15);

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);
// 'heart-attack', 30x30px
const unsigned char epd_bitmap_heart_attack [] PROGMEM = {
   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0xe0, 0x1f, 0x00, 0x0f, 0xf8, 0x7f, 0xc0, 
   0x1f, 0xfc, 0xff, 0xe0, 0x3f, 0xff, 0xff, 0xf0, 0x7f, 0xff, 0xff, 0xf8, 0x7f, 0xff, 0xff, 0xf8, 
   0x7f, 0xff, 0xff, 0xf8, 0xff, 0xff, 0xff, 0xfc, 0xfe, 0xfc, 0xff, 0xfc, 0xfc, 0x7c, 0xff, 0xfc, 
   0xfc, 0x78, 0x7f, 0xfc, 0x78, 0x38, 0x7f, 0xf8, 0x00, 0x30, 0x00, 0xf8, 0x01, 0x13, 0x00, 0x78, 
   0x3f, 0x83, 0xff, 0xf0, 0x3f, 0x87, 0xff, 0xf0, 0x1f, 0xc7, 0xff, 0xe0, 0x0f, 0xcf, 0xff, 0xc0, 
   0x07, 0xff, 0xff, 0x80, 0x03, 0xff, 0xff, 0x00, 0x01, 0xff, 0xfe, 0x00, 0x00, 0xff, 0xfc, 0x00, 
   0x00, 0x7f, 0xf8, 0x00, 0x00, 0x3f, 0xf0, 0x00, 0x00, 0x0f, 0xc0, 0x00, 0x00, 0x07, 0x80, 0x00, 
   0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
};

// Array of all bitmaps for convenience. (Total bytes used to store images in PROGMEM = 144)
const int epd_bitmap_allArray_LEN = 1;
const unsigned char* epd_bitmap_allArray[1] = {
  epd_bitmap_heart_attack
};


DFRobot_Heartrate heartrate(DIGITAL_MODE);

int stepCount = 0;
int oldStepCount = 0;
int rate = 0;
bool btconnected = false;
bool heartScanning = true;

void updatePedoCount(){
  float x,y,z;
  if (IMU.accelerationAvailable()){
    IMU.readAcceleration(x,y,z);
  }
  float accVector;
  accVector = pow(x,2) + pow(y,2) + pow(z,2);
  if (accVector > 4.5)
  {
    stepCount++;
    Serial.println(stepCount);
  }
  // if (stepCount != oldStepCount) {
  pedoCount.writeValue(String(stepCount)+","+String(rate));
    // oldStepCount = stepCount;
  // }
}

void checkHeartRate(){
  uint8_t rateValue;
  heartrate.getValue(heartratePin);   // A1 foot sampled values
  rateValue = heartrate.getRate();   // Get heart rate value 
  if(rateValue)  {
    heartScanning = false;
    rate = rateValue;
  }
  // if(rateValue == 0)
  //   heartScanning = true;
  delay(20);
}

void displayFunction()
{
  // clear the display:
  display.clearDisplay();
  // set the text size to 5:
  display.setTextSize(2);
  // set the text color to white:
  display.setTextColor(SSD1306_WHITE);
  // move the cursor to 0,0:
  display.setCursor(0, 5);
  display.print("Counts: ");
  display.print(stepCount);
  display.drawBitmap(0, 35, epd_bitmap_heart_attack, 30, 30, WHITE);
  display.setCursor(50, 38);
  if(!heartScanning)
    display.print(rate);
  else
    display.print("--");

  display.display();
}

void displayStart()
{  
  if(!btconnected)
  {
    display.clearDisplay();
    display.setTextSize(1);
    display.setTextColor(SSD1306_WHITE);
    display.setCursor(20,30);

    display.print("BT Disconnected");
    display.display();
  }
}

// void btstart()
// {
//   display.clearDisplay();
//   display.setTextSize(1);
//   display.setTextColor(SSD1306_WHITE);
//   display.setCursor(20,30);
//   display.print("BT Connected");
//   display.display();
//   delay(1000);
//   btconnected = true;
// }


void setup() {
  Serial.begin(115200);

   //////////////////////OLED SETUP//////////////////////////
   if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) {
     Serial.println("Display setup failed");
     while(true);
   }
   // clear the display:
   display.clearDisplay();
  //////////////////////IMU SETUP//////////////////////////
  if (!IMU.begin()) {
    Serial.println("Failed to initialize IMU!");
    while (1);
  }
  //////////////////////
```

## How to Use
