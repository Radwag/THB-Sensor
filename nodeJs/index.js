const SerialPort = require("serialport");
let allPorst = [];
let thbPorst = [];

async function getSerialPorts() {
  await SerialPort.list().then((ports) => {
    ports.forEach((port) => {
      allPorst.push(port.path);
    });
  });
}

async function checkPorts(allFindedPorts) {
  const promises = [];
  for (port of allFindedPorts) {
    await askPort(port, "RMX.WHOIS\r\n")
      .then((message) => {
        if (message.search("HW_MODEL=THB_SENSOR") > 0) {
          thbPorst.push(port);
        }
      })
      .catch((message) => {});
  }
}

function askPort(portName, message) {
  return new Promise(function (resolve, reject) {
    const SerialPort = require("serialport");
    const Readline = require("@serialport/parser-readline");
    const port = new SerialPort(portName, {
      baudRate: 115200,
    });
    const parser = new Readline();
    port.pipe(parser);

    port.on("error", (err) => {
      reject("error" + portName + err.message);
    });
    parser.on("data", (_data) => {
      port.close();
      resolve(_data);
    });
    setTimeout(() => {
      reject("rejected by timeout");
    }, 2000);
    port.write(message, function (err) {
      if (err) {
        reject("error" + err.message);
      }
    });
  });
}

async function App() {
  console.log("=========Finding all COM Ports===============");
  await getSerialPorts();
  console.log(allPorst);
  console.log("=======Checking THB Devices=========");
  await checkPorts(allPorst);
  console.log(thbPorst);
  if (thbPorst.length === 0) {
    console.log("No THB Devices Finded");
    process.exit(0);
  }
  console.log("====Asking for Data=======");
  const interval = require("interval-promise");

  await interval(async () => {
    for (port of thbPorst) {
      await askPort(port, "THB.CURR\r\n")
        .then((message) => {
          console.log(port + " " + message);
        })
        .catch((message) => {
          console.log(message);
        });
    }
  }, 2000);
}

App();