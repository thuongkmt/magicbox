const { Console } = require('winston/lib/winston/transports');

var oldTimeout;

function showDialog(msg, timeout) {
    $("#modalMsg").modal("hide");
    clearTimeout(oldTimeout);

    if (msg != "") {
        $("span.dialog-message").html(msg);
        $('#modalMsg').modal({
            keyboard: false,
            backdrop: true
        });

        if (timeout !== 0) {
            oldTimeout = setTimeout(function () {
                $("#modalMsg").modal("hide");
            }, timeout * 1000);
        }
    }
}

$(document).ready(function () {
    console.log("READY");
    getHwVersion(function (data) {
        console.log("hwVersion: " + data);
        $("#version").html(data);
    });
    $("#modalMsg").modal("hide");

    GetConfig('RfidFridgeSetting.CustomerUI.Currency', function (data) {
        console.log("currency data: " + data)
        formatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: data,
        });
        console.log("formatter:" + formatter)
    });


    GetConfig('RfidFridgeSetting.CustomerUI.Messages.TapCard', function (data) {
        $("#instruction-message").html(data);
        var message = data.split("<br>");
        let html = '';
        message.forEach(element => {
            console.log(element);
            html += "<span>" + element + "</span>"
        });
        $("#title-tap-insert").html(html);
    });

    GetConfig('RfidFridgeSetting.Machine.BottomText', function (data) {
        $("#bottomText").html(data);
    });

    GetConfig('RfidFridgeSetting.Machine.AdvsImage', function (data) {
        $("#advsImg").attr("src", data)
    });

    GetConfig('RfidFridgeSetting.System.Cloud.CloudApiUrl', function (data) {
        cloudUrl = data;
        console.log("cloudUrl:" + cloudUrl);
    });

    GetConfig('RfidFridgeSetting.Machine.Id', function (data) {
        machineId = data;
        console.log("machineId:" + machineId);
    });

    GetConfig('RfidFridgeSetting.System.Cloud.TenantId', function (data) {
        tenantId = data;
        console.log("tenantId:" + tenantId);
    });

    GetConfig('RfidFridgeSetting.System.Inventory.CartUIDelay', function (data) {
        inventoryCartUiDelay = parseInt(data);
        console.log("inventoryCartUiDelay:" + inventoryCartUiDelay);
    });

    GetConfig('RfidFridgeSetting.System.Cloud.MapTagByPrefix', function (data) {
        usePrefixMethod = (data.toLowerCase() === 'true');
        console.log("MapTagByPrefix:" + usePrefixMethod);
    });

    GetConfig('RfidFridgeSetting.System.Payment.Magic.TerminalType', function (data) {
        console.log("Terminal Type: " + data)
        if(!data.toLowerCase() === 'IM30'.toLowerCase())
        {
            $("#btn-start").hide();
        }else {
            
        }
        
    });





    // SUB STORM MESSAGE
    var amount = 0;
    var intervalMsg;
    SubMsg(function (msg) {
        var data = JSON.parse(msg);
        var contentHtml = "";
        var products = [];
        data.Inventories.forEach(element => {
            // contentHtml += "<tr><td>" + element.Product.ProductName + "</td><td>$" + element
            //     .Price +
            //     "</td></tr>"
            products.push({
                product: element.Product.ProductName,
                price: element.Product.Price
            });
        });
        amount = data.Amount;
        //$("#table_content_order").html(contentHtml);
        // $("#total").text("Total: " + data.Amount);

        console.log("lockRefreshTable: " + lockRefreshTable);
        if (!lockRefreshTable) {
            setTotalCharged(data.Amount);
            addProductsToShoppingCart(products)
        }

    }, function (msg) {
        var data = JSON.parse(msg);
        var contentHtml = "";
        data.forEach(element => {
            contentHtml += "<tr><td>" + element.Product.ProductName + "</td><td>$" + element
                .Price +
                "</td></tr>"
        });
        $("#table_content_inventory").html(contentHtml);
    }, function (msg) {
        var data = JSON.parse(msg);
        if (data.Status == "TRANSACTION_DONE") {
            var formatedAmount = formatter.format(amount);
            if (amount === 0) {
                window.location.href =
                    "pages/message7inch.html?msg=PURCHASE CANCELLED<br>YOU WILL NOT BE CHARGED";
            } else {
                window.location.href =
                    "pages/message7inch.html?msg=YOU HAVE BEEN CHARGED: <span style='color: orange'>" +
                    formatedAmount + "</span>";
            }
        }
        if (data.Status == "TRANSACTION_START" || data.Status == "TRANSACTION_REOPENDOOR") {
            showSelectProductsLayout();
        }
        if (data.Status == "PREAUTH_MAKE_PAYMENT" || data.Status == "PREAUTH_REFUND") {
            $("#title-message-screen").html(data.Message);
            showMessageScreenLayout();
        }
        if (data.Status == "TRANSACTION_CONFIRM") {
            enableButtons();
            showShoppingCartLayout();
        }
        if (data.Status == "STOPSALE" ||
            data.Status == "MANUAL_STOPSALE" ||
            data.Status == "UNSTABLE_TAGS_DIAGNOSTIC" ||
            data.Status == "UNSTABLE_TAGS_DIAGNOSTIC_TRACING"
            //data.Status == "UNLOADING_PRODUCT"
        ) {
            window.location.href = "pages/message-notimeout.html?msg=" + data.Message;
        }
        if (data.Status == "VALIDATE_CARD_FAILED") {
            window.location.href =
                "pages/message7inch.html?msg=" + data.Message;
        }
        if (data.Status == "TRANSACTION_WAITTING_FOR_PAYMENT") {
            window.location.href =
                "pages/messagemessage7inch.html?msg=PLEASE WAIT<br>WE ARE FINISHING...";
        }
        if (data.Status == "STOPSALE_DUE_TO_PAYMENT") {
            window.location.href =
                "pages/message-notimeoutmessage7inch.html?msg=" + data.Message;
        }
    }, function (msg) {
        var data = JSON.parse(msg);
        $("#temperature").text(Math.round(data.Temp) + "°C");
    }, function (message) {
        var data = JSON.parse(message);
        $("#instruction-message").text(data);
    }, function (message) {
        var data = JSON.parse(message);
        var dialogMessage = data.Message;
        var dialogTimeout = data.Timeout;
        clearInterval(intervalMsg);
        $(Spinner_Loading_Sale).hide();
        if (dialogMessage ==
            'WE ARE VALIDATING YOUR CARD<BR><b>PLEASE DO NOT TAKE OUT YOUR CARD</b>') {
            $("#modalMsg").modal("hide");
            clearTimeout(oldTimeout);
            var msg = dialogMessage.replace('<BR>', '.');
            msg = msg.replace('<b>', ' ');
            msg = msg.replace('</b>', '');
            $(Spinner_Loading_Sale_Msg).text(msg);
            $(Spinner_Loading_Sale).show();

            let countdown = dialogTimeout * 1000;
            let second = 1000;
            clearInterval(intervalMsg);
            intervalMsg = setInterval(() => {
                countdown -= second;
                if (countdown === second) {
                    clearInterval(intervalMsg);
                    $(Spinner_Loading_Sale).hide();
                }
            }, second);
        } else {
            showDialog(dialogMessage, dialogTimeout);
        }
    });
});

SubTagId(function (msg) {
    var data = JSON.parse(msg);
    let newTags = [];
    let newRemovedTags = [];
    data.forEach(d => {
        // added
        if (d.State == 0) {
            newTags.push(d.Id);
        }
        // removed
        if (d.State == 1) {
            newRemovedTags.push(d.Id);
        }
    });
    tags = newTags;
    removedTags = newRemovedTags;

    logger.info("data:" + JSON.stringify(data));
    // logger.info("tags:"+JSON.parse(tags));
    //  logger.info("removedTags:"+JSON.parse(removedTags));

    processRestockData(data);

    $("#total-items-restock").html("Total Restock: " + tags.length);
    $("#total-items-unload").html("Total Unload: " + removedTags.length);

    // Dump for test
    //tags.push('E00403500E151A66');
    //console.log(tags);

});

SubMissedInventory(function (msg) {
    var data = JSON.parse(msg);

    console.log("Missed inven:" + data);
    var table = $("#table_unload > tbody");
    console.log(table);
    //table.html("");
    var body = "";
    var unloadIds = [];
    data.forEach(x => {
        body += "<tr> <td>" + x.Product.ProductName + "</td> <td>" + x.TagId + "</td> </tr>"
        unloadIds.push(x.Id);
    });
    dataUnload = unloadIds;

    $("#total-items-unload").html("Total: " + dataUnload.length);

    console.log(body);
    table.html(body)
});

document.addEventListener("keydown", function (e) {
    if (e.which === 123) {
        require('remote').getCurrentWindow().toggleDevTools();
    } else if (e.which === 116) {
        location.reload();
    }
});