#include <Servo.h>

const int X_PIN = 26;
const int Y_PIN = 27;
const int Z_PIN = 28;
const int SERVO_PIN = 15;

Servo myServo;

struct Coordinate {
  long cx, cy, cz;
  long intensity;
};

struct Angle {
  int ax, ay, az;
  int power;
};

const int N = 100;

void setup() {
  Serial.begin(115200);
  analogReadResolution(12);

  myServo.attach(SERVO_PIN);
  myServo.attach(SERVO_PIN, 500, 2400);
  myServo.write(0);
}

void loop() {
  Coordinate c = getCoordinate();
  Angle a = angleCalculation(c);

  String message = String(a.ax) + "," + String(a.ay) + "," + String(a.az) + "," + String(a.power);
  Serial.println(message);

  // 受信
  if (Serial.available() > 0) {
    String input = Serial.readStringUntil('\n');
    int progress = input.toInt();

    int angle = map(progress, 0, 100, 0, 180);

    if (progress >= 0 && progress <= 100){
      myServo.write(angle);
    }
  }

  delay(20);
}

// 加速度センサの値を取得する関数
Coordinate getCoordinate() {
  Coordinate c;
  long x = 0, y = 0, z = 0;
  long minX = 4096, maxX = 0;
  long minY = 4096, maxY = 0;
  long minZ = 4096, maxZ = 0;

  for (int i = 0; i < N; i++) {
    long rx = analogRead(X_PIN);
    long ry = analogRead(Y_PIN);
    long rz = analogRead(Z_PIN);

    x += rx;
    y += ry;
    z += rz;

    // この期間内での最大・最小を記録
    if(rx < minX) minX = rx; if(rx > maxX) maxX = rx;
    if(ry < minY) minY = ry; if(ry > maxY) maxY = ry;
    if(rz < minZ) minZ = rz; if(rz > maxZ) maxZ = rz;
  }

  c.cx = x / N;
  c.cy = y / N;
  c.cz = z / N;

  c.intensity = (maxX - minX) + (maxY - minY) + (maxZ - minZ);

  return c;
}

// センサの値から各座標の角度を計算する関数
Angle angleCalculation(Coordinate c) {
  int MAX_X = 3105, MAX_Y = 3130, MAX_Z = 3175;
  int MIN_X = 1910, MIN_Y = 1940, MIN_Z = 1915;

  float oneAngleX = (MAX_X - MIN_X) / 180.000;
  float oneAngleY = (MAX_Y - MIN_Y) / 180.000;
  float oneAngleZ = (MAX_Z - MIN_Z) / 180.000;

  Angle a;
  a.ax = (c.cx - MIN_X) / oneAngleX - 90;
  a.ay = (c.cy - MIN_Y) / oneAngleY - 90;
  a.az = (c.cz - MIN_Z) / oneAngleZ - 90;

  a.power = c.intensity / 10;

  return a;
}

void showAngle(Angle a) {
  String message = String(a.ax) + ", " + String(a.ay) + ", " + String(a.az);
  Serial.println(message);
}

void showCoordinate(Coordinate c) {
  String message = "x: " + String(c.cx) + " y: " + String(c.cy) + " z: " + String(c.cz);
  Serial.println(message);
}