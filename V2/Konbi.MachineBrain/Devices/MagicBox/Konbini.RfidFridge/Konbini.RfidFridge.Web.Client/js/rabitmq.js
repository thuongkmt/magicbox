var amqp = require('amqplib/callback_api');

function RbmqSub(onDataRevc) {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var q = 'hello';

      ch.assertQueue(q, {
        durable: false
      });
      console.log(" [*] Waiting for messages in %s. To exit press CTRL+C", q);
      ch.consume(q, function (msg) {
        onDataRevc(msg.content.toString());
        console.log(" [x] Received %s", msg.content.toString());
      }, {
        noAck: true
      });
    });
  });

}

function RbmqPub(msg) {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var q = 'hello';

      ch.assertQueue(q, {
        durable: false
      });
      ch.sendToQueue(q, Buffer.from(msg));
      console.log(" [x] Sent %s", msg);
    });
    //onclick="OpenLock()"
    setTimeout(function () {
      conn.close();
    }, 500);
  });
}

function SubTagIdExchange(onDataRevc) {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var ex = 'tagid';

      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for tagid');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
  });
}

function SubMachineStatusExchange(onDataRevc) {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var ex = 'machinestatus';

      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for machinestatus');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
  });
}


function PubTagIdExchange() {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var ex = 'tagid';
      ch.assertExchange(ex, 'fanout', {
        durable: false
      });
      ch.publish(ex, "", new Buffer(msg));
      console.log(" [>] Sent '%s'", msg);
    });

    setTimeout(function () {
      conn.close();
    }, 500);
  });
}

function SubInventory(onDataRevc) {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var ex = 'inventory';

      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for inventory');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
  });
}

function SubOrder(onDataRevc) {
  amqp.connect('amqp://localhost', function (err, conn) {
    conn.createChannel(function (err, ch) {
      var ex = 'order';

      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for order');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
  });
}

function SubMsg(onDataRevc_order,onDataRevc_inven,onDataRevc_machinestatus) {
  console.log("SUB MSG");
  amqp.connect('amqp://localhost', function (err, conn) {
    console.log("CONNECT");
    conn.createChannel(function (err, ch) {
      var ex = 'order';
      console.log("CHANNEL 1");
      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for order');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc_order(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
    //---------------------------------
    conn.createChannel(function (err, ch) {
      var ex = 'inventory';
      console.log("CHANNEL 2");
      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for inventory');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc_inven(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
    //-----------------------------------
    conn.createChannel(function (err, ch) {
      var ex = 'machinestatus';
      console.log("CHANNEL 3");
      ch.assertExchange(ex, 'fanout', {
        durable: false
      });

      ch.assertQueue('', {
        exclusive: true
      }, function (err, q) {
        console.log(' [*] Waiting for machinestatus');
        ch.bindQueue(q.queue, ex, "");
        ch.consume(q.queue, function (msg) {
          onDataRevc_machinestatus(msg.content.toString());
          console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
        }, {
          noAck: true
        });
      });
    });
  });
}