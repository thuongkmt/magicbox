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

var isRabbitMqConnected = false;
function CheckRabbitMqConnection() {
    var isError = false;
    var isDone = false;
    amqp.connect('amqp://localhost', function (err, conn) {
        if (err != null) {
            isError = true;
            isRabbitMqConnected = false;
        } else {
            isError = false;
            isRabbitMqConnected = true;
        }
        isDone = true;
    });

    // while (!isDone) {
    //     sleep(2000).then(() => { console.log("Waitting for rabbitmq connection..."); });
    // }
    return isRabbitMqConnected;
}


function SubDeviceChecking(onDataRevc) {
    amqp.connect('amqp://localhost', function (err, conn) {
        if (err != null) {
            sleep(5000);
            location.reload();
        }
        conn.createChannel(function (err, ch) {
            var ex = 'device-checking';

            ch.assertExchange(ex, 'fanout', {
                durable: false
            });

            ch.assertQueue('', {
                exclusive: true
            }, function (err, q) {
                console.log(' [*] Waiting for device checking data');
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
        if (err != null) {
            sleep(5000);
            location.reload();
        }
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

function SubTagId(onDataRevc) {
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

function SubMissedInventory(onDataRevc) {
    amqp.connect('amqp://localhost', function (err, conn) {
        conn.createChannel(function (err, ch) {
            var ex = 'missed-inventory';

            ch.assertExchange(ex, 'fanout', {
                durable: false
            });

            ch.assertQueue('', {
                exclusive: true
            }, function (err, q) {
                console.log(' [*] Waiting for missed-inventory');
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


function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function SubMsg(onDataRevc_order, onDataRevc_inven, onDataRevc_machinestatus, onDataRevc_temp, onDataRevc_mess, onDataRevc_dialogmess) {
    console.log("SUB MSG");
    amqp.connect('amqp://localhost', function (err, conn) {
        //alert("err: "+err)
        if (err != null) {
            sleep(5000);
            location.reload();
        }
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
                    // console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
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
        //-----------------------------------
        conn.createChannel(function (err, ch) {
            var ex = 'temperatures';
            console.log("CHANNEL 4");
            ch.assertExchange(ex, 'fanout', {
                durable: false
            });

            ch.assertQueue('', {
                exclusive: true
            }, function (err, q) {
                console.log(' [*] Waiting for temperatures');
                ch.bindQueue(q.queue, ex, "");
                ch.consume(q.queue, function (msg) {
                    onDataRevc_temp(msg.content.toString());
                    // console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
                }, {
                    noAck: true
                });
            });
        });
        //-----------------------------------
        conn.createChannel(function (err, ch) {
            var ex = 'messages';
            console.log("CHANNEL 5");
            ch.assertExchange(ex, 'fanout', {
                durable: false
            });

            ch.assertQueue('', {
                exclusive: true
            }, function (err, q) {
                console.log(' [*] Waiting for message');
                ch.bindQueue(q.queue, ex, "");
                ch.consume(q.queue, function (msg) {
                    onDataRevc_mess(msg.content.toString());
                    console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
                }, {
                    noAck: true
                });
            });
        });
        //-----------------------------------
        conn.createChannel(function (err, ch) {
            var ex = 'dialog-messages';
            console.log("CHANNEL 6");
            ch.assertExchange(ex, 'fanout', {
                durable: false
            });

            ch.assertQueue('', {
                exclusive: true
            }, function (err, q) {
                console.log(' [*] Waiting for dialog-message');
                ch.bindQueue(q.queue, ex, "");
                ch.consume(q.queue, function (msg) {
                    onDataRevc_dialogmess(msg.content.toString());
                    console.log(" [x] %s: '%s'", msg.fields.routingKey, msg.content.toString());
                }, {
                    noAck: true
                });
            });
        });
    });
}