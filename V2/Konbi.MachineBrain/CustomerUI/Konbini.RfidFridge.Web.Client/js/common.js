const { loggers } = require("winston");

const Screen_Sale = 'screen-sale';
const Screen_Restock = 'screen-restock';

const Tap_Insert = 'tap-insert';
const Message_Screen = 'message-screen';

const Select_Products = 'select-products';
const Shopping_Cart = 'shopping-cart';
const Footer = 'footer';
const Have_Been_Charged = 'have-been-charged';

const Layout_Passcode = 'layout-passcode';
const Layout_Option = 'layout-option';
const Layout_restock = 'layout-restock';
const Layout_unload = 'layout-unload';
const Layout_Result = 'layout-result';

const Total_Charged = '#total-charged';
const Total_Value = '#total-value';
// const Content_Products = '#content-products';
// const Content_Prices = '#content-prices';
const Content_Shoppingcart = '#content-shoppingcart';
const Content_Welcome = '#content-welcome';
const Title_Result = '#title-result';
const Content_Result = '#content-result';
const Title_Button = '#title-button';
const Table_Restock = '#table-restock';
const Table_Unload = '#table_unload';
const Spinner_Loading = '#spinner-loading';
const Spinner_Loading_Sale = '#spinner-loading-sale';
const Spinner_Loading_Sale_Msg = '#spinner-loading-sale-msg';
const Total_Items_Restock = '#total-items-restock';
const Total_Items_Unload = '#total-items-unload';
const Count_Down_Payment = '#count-down-payment';

let cloudUrl = 'http://174.138.182.44:22765';
let machineId = '7b0efd48-04e6-6f5d-5b2a-790275369f64';
let lengthPasscode = 6
let clickChange = 0;
let inputPassCode = '';
let tenantId = '5';
let passCode = '000000';
let employee = 'Admin';
let isUnload = true;
let tags = [];
let removedTags = [];
let isRestock = false;
let isGetProduct = false;
let unloadLoading = false;
let URL_CONFIRM_RESTOCK = 'http://localhost:22742/api/services/app/Inventories/RestockWithRestockerName';
let URL_CONFIRM_UNLOAD = 'http://localhost:22742/api/services/app/Inventories/UnloadItems';
let URL_CONFIRM_RESTOCKSESSION = 'http://localhost:22742/api/services/app/Inventories/RestockWithSession';
let usePrefixMethod = true;
let dataRestock = [];
let dataUnload = [];
let lockRefreshTable = false;
const second = 1000;
const timeWaitAutoPayment = 15000;
const timeWaitAutoBackToSale = 60000;
let intervalCountdown;
let inventoryCartUiDelay = 6000;
let cartDelayTimer;
/**
 * Create our number formatter.
 */

let formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
});

/**
 * Document is ready.
 */
$(document).ready(() => {

    logger.info("READY");
    showHideLayout(Tap_Insert, true);
    showHideLayout(Select_Products, false);
    showHideLayout(Shopping_Cart, false);
    showHideLayout(Footer, true);
    showHideLayout(Have_Been_Charged, false);

    showHideLayout(Screen_Sale, true);
    showHideLayout(Screen_Restock, false);

    showHideLayout(Layout_Passcode, false);
    showHideLayout(Layout_Option, false);
    showHideLayout(Layout_restock, false);
    showHideLayout(Layout_unload, false);
    showHideLayout(Layout_Result, false);
    showHideLayout(Message_Screen, false);

    $(Spinner_Loading).hide();
    $(Spinner_Loading_Sale).hide();
});

/**
 * Show Hide Layout.
 * @param {string} element - Element Html.
 * @param {bool} show - Status show or hide: True is show, False is hide.
 */
function showHideLayout(element, show) {
    if (show) {
        $('#' + element).show();
    } else {
        $('#' + element).hide();
    }

    if (element === Layout_Passcode) {
        if (show) {
            logger.info('Create Set Interval Back To Sale.');
            let countdown = timeWaitAutoBackToSale;
            clearInterval(intervalCountdown);
            intervalCountdown = setInterval(() => {
                countdown -= second;
                if (countdown === 0) {
                    clearInterval(intervalCountdown);
                    backToSale();
                }
            }, second);
        } else {
            logger.info('Remove Set Interval Back To Sale.');
            clearInterval(intervalCountdown);
        }

    }
}

function disablePaymentButtons() {
    //$("#btn-opendoor").addClass('disabled');
    $("#btn-makepayment").addClass('disabled');
}


function disableButtons() {
    logger.info("Disable buttons");

    $("#btn-opendoor").addClass('disabled');
    $("#btn-makepayment").addClass('disabled');
}

function enableButtons() {
    logger.info("Enable buttons");

    $("#btn-opendoor").removeClass('disabled');
    $("#btn-makepayment").removeClass('disabled');
}
let hwControllerVersion = "1.0";

function getHwVersion(callback) {
    logger.info("Get HwController version");
    $.get("http://localhost:9000/api/machine/version", function (data) {

        logger.info("version: " + data);
        callback(data);
    })

}


function getCurrentStatus(callback) {
    logger.info("Get Current Machine Status");
    $.get("http://localhost:9000/api/machine/status", function (data) {

        logger.info("Current Machine Status: " + data);
        callback(data);
    }).fail(() => {

    });

}
var isHwApiStarted =  false;
function CheckLocalHwApi() {
    $.get("http://localhost:9000/api/machine/status", function (data) {

        logger.info("Current Machine Status: " + data);
        isHwApiStarted = true;
    }).fail(() => {
        isHwApiStarted = false;
    });
    return isHwApiStarted;
}



/**
 * Event Open Door.
 */
function openDoor() {
    logger.info("Open Door");
    clearInterval(intervalCountdown);
    clearInterval(cartDelayTimer);

    disableButtons();
    $.post("http://localhost:9000/api/machine/reopen", {}, (result) => { })
        // Success.
        .done(() => {
            logger.info("success.");
        })
        // Error.
        .fail(() => {
            logger.info("error.");
        })
        // finally.
        .always(() => {
            logger.info("finished.");
        });
}

/**
 * Event Open Door.
 */
function manualOpenDoor() {
    logger.info("Open Door");
    clearInterval(intervalCountdown);
    disableButtons();
    $.post("http://localhost:9000/api/machine/open", {}, (result) => { })
        // Success.
        .done(() => {
            logger.info("success.");
        })
        // Error.
        .fail(() => {
            logger.info("error.");
        })
        // finally.
        .always(() => {
            logger.info("finished.");
        });
}

function backToSale() {
    setMachineStatus("IDLE");
    showHideLayout(Footer, true);

    changeLayout();
}

/**
 * Event Open Door.
 */
function setMachineStatus(status) {
    logger.info("Set machine status");
    disableButtons();
    $.post("http://localhost:9000/api/machine/setstatus/" + status, {}, (result) => {
        logger.info(result)
    })
        // Success.
        .done(() => {
            logger.info("success.");
        })
        // Error.
        .fail(() => {
            logger.info("error.");
        })
        // finally.
        .always(() => {
            logger.info("finished.");
        });
}

function reloadInventory() {
    logger.info("Reload inventory");
    disableButtons();
    $.post("http://localhost:9000/api/machine/inventory/reload/", {}, (result) => {
        logger.info(result)
    })
        // Success.
        .done(() => {
            logger.info("reloadInventory success.");
        })
        // Error.
        .fail(() => {
            logger.info("reloadInventory error.");
        })
        // finally.
        .always(() => {
            logger.info("reloadInventory  finished.");
        });
}

function cleanRestockData() {
    logger.info("Clean Restock Data");
    disableButtons();
    $.post("http://localhost:9000/api/machine/inventory/cleanrestockdata/", {}, (result) => {
        logger.info(result)
    })
        // Success.
        .done(() => {
            logger.info("cleanrestockdata success.");
        })
        // Error.
        .fail(() => {
            logger.info("cleanrestockdata error.");
        })
        // finally.
        .always(() => {
            logger.info("cleanrestockdata  finished.");
        });
}



/**
 * Event Make Payment.
 */
function makePayment() {
    clearInterval(intervalCountdown);
    $(Count_Down_Payment).text('');
    // Start the loading spinner.
    if ($(Total_Value).text().trim() === 'Total: ' + formatter.format(parseFloat(0))) {
        $(Spinner_Loading_Sale_Msg).text('Please wait...');
    } else {
        $(Spinner_Loading_Sale_Msg).text('Processing Payment. Please wait...');
    }
    $(Spinner_Loading_Sale).show();

    logger.info("Make Payment");
    disableButtons();
    $('#msg-false').remove();

    $.post("http://localhost:9000/api/machine/endtransaction", {}, (result) => { })
        // Success.
        .done(() => {
            logger.info("success.");
            if ($(Total_Value).text().trim() === 'Total: ' + formatter.format(parseFloat(0))) {
                showMessage(true, 'Cancel success.', Shopping_Cart);
                //  disableButtons();

            } else {
                showMessage(true, 'Payment success.', Shopping_Cart);
                // disableButtons();

            }

        })
        // Error.
        .fail(() => {
            logger.info("error.");
            showMessage(false, 'Payment error.', Shopping_Cart);
        })
        // finally.
        .always(() => {
            logger.info("finished.");
            //alert('LOL');
            //$(Spinner_Loading_Sale).hide();
            disableButtons();

            // enableButtons();
        });

}

/**
 * Set Total Charged.
 * @param {number} totalCharged - Total Charged Value.
 */
function setTotalCharged(totalCharged) {
    $(Total_Charged).text(formatter.format(totalCharged));
}

/**
 * Set Total Value.
 * @param {*} totalValue - Total value of products list.
 */
function setTotalValue(totalValue) {
    $(Total_Value).text('Total: ' + formatter.format(parseFloat(totalValue)));
}

/**
 * Add Products To Shopping Cart.
 * @param {Array} data - Array products and prices.
 */
function addProductsToShoppingCart(data) {
    var contentShoppingcartHtml = "";
    var totalValue = 0;

    // var index = 0;
    data.forEach(element => {
        contentShoppingcartHtml += `
        <div class="row">
            <div class="col-8" style="text-align: left; padding-left: 2rem; font-size: 2.5rem;">
            ` + element.product + `
            </div>
            <div class="col-4" style="text-align: right; padding-right: 2rem; font-size: 2.5rem;">
                ` + formatter.format(parseFloat(element.price)) + `
            </div>
        </div>
        `;
        totalValue += element.price;
    });

    contentShoppingcartHtml += `
        <div style="height: 60vh; width: 2px; background-color: #fff; position: fixed; top: 85px; left: 66.66%;"></div>
    `;

    setTotalValue(totalValue);
    $(Content_Shoppingcart).html(contentShoppingcartHtml);
}

/**
 * Show Tap/Insert Layout.
 */
function showTapInsertLayout() {
    showHideLayout(Tap_Insert, true);
    showHideLayout(Select_Products, false);
    showHideLayout(Shopping_Cart, false);
    showHideLayout(Footer, true);
    showHideLayout(Have_Been_Charged, false);
}

/**
 * Show Select Products Layout.
 */
function showSelectProductsLayout() {
    showHideLayout(Tap_Insert, false);
    showHideLayout(Select_Products, true);
    showHideLayout(Shopping_Cart, false);
    showHideLayout(Footer, false);
    showHideLayout(Have_Been_Charged, false);
    logger.info("Cleared cart timer")
    clearInterval(cartDelayTimer);
}

/**
 * Show Shopping Cart Layout.
 */
function showShoppingCartLayout() {
    disablePaymentButtons();
    setTotalValue(0);
    showHideLayout(Tap_Insert, false);
    showHideLayout(Select_Products, false);
    showHideLayout(Shopping_Cart, true);
    showHideLayout(Footer, false);
    showHideLayout(Have_Been_Charged, false);

    logger.info("Cleared cart timer")
    clearInterval(cartDelayTimer);
    clearCartAndShowLoading();
}

function showMessageScreenLayout() {
    showHideLayout(Tap_Insert, false);
    showHideLayout(Select_Products, false);
    showHideLayout(Shopping_Cart, false);
    showHideLayout(Footer, false);
    showHideLayout(Have_Been_Charged, false);
    showHideLayout(Message_Screen, true);
    $(Spinner_Loading_Sale).hide();
}

/**
 * Show Have Been Charged Layout.
 */
function showHaveBeenChargedLayout() {
    showHideLayout(Tap_Insert, false);
    showHideLayout(Select_Products, false);
    showHideLayout(Shopping_Cart, false);
    showHideLayout(Footer, false);
    showHideLayout(Have_Been_Charged, true);
}

/**
 * Event change layout Restock or Sale.
 */
function eventChangeLayout() {

    if ($('#' + Tap_Insert).is(":visible") || $('#' + Layout_Passcode).is(":visible")) {
        clickChange++;
        if (clickChange === 3) {
            changeLayout();
            clickChange = 0;
        }
    }

    return;
}

/**
 * Toggle layout Restock and Sale.
 */
function changeLayout() {
    if ($('#' + Screen_Sale).is(':visible')) {
        showHideLayout(Screen_Sale, false);
        showHideLayout(Screen_Restock, true);

        showHideLayout(Layout_Passcode, true);
        showHideLayout(Layout_Option, false);
        showHideLayout(Layout_restock, false);
        showHideLayout(Layout_unload, false);
        showHideLayout(Layout_Result, false);
    } else {
        showHideLayout(Screen_Sale, true);
        showHideLayout(Screen_Restock, false);
    }
    return;
}

/**
 * Enter key of passcode.
 * @param {*} value 
 */
function enterButton(value) {
    if (value === 66) {
        inputPassCode = '';
        for (let index = 1; index < lengthPasscode + 1; index++) {
            $('#passcode' + index).css('color', '#727B86');
        }
    } else if (value === 88 && inputPassCode.length > 0) {
        let index = inputPassCode.length;
        inputPassCode = inputPassCode.substring(0, inputPassCode.length - 1);
        $('#passcode' + index).css('color', '#727B86');
    } else {
        if (inputPassCode.length < lengthPasscode && value < 10) {
            inputPassCode += value;
            $('#passcode' + inputPassCode.length).css('color', '#fff');
        }
    }

    if (inputPassCode.length < 6) {
        return;
    }

    // Start the loading spinner.
    $(Spinner_Loading).show();
    $.get(cloudUrl + '/api/TokenAuth/GetTokenByPassCode', {
        'passCode': inputPassCode,
        'tenantId': tenantId
    })
        // Success.
        .done((success) => {
            logger.info(success);
            if (!success.result) {
                showMessage(false, 'Wrong passcode!!', Layout_Passcode);
                setTimeout(() => {
                    inputPassCode = '';
                    for (let index = 1; index < lengthPasscode + 1; index++) {
                        $('#passcode' + index).css('color', '#727B86');
                    }
                }, 1000);
                return;
            } else {
                logger.info("Login success.");
                employee = success.result.userName;
                inputPassCode = '';
                for (let index = 1; index < lengthPasscode + 1; index++) {
                    $('#passcode' + index).css('color', '#727B86');
                }
                showHideLayout(Layout_Passcode, false);
                showHideLayout(Layout_Option, true);

                $(Content_Welcome).text('Welcome ' + employee);
            }
        })
        // Error.
        .fail(() => {
            logger.info("Login Error.");
            showMessage(false, 'Can not connect to cloud!!!', Layout_Passcode);
            inputPassCode = '';
            for (let index = 1; index < lengthPasscode + 1; index++) {
                $('#passcode' + index).css('color', '#727B86');
            }
        })
        // finally.
        .always(() => {
            logger.info("Login Finished.");
            // Finish the loading spinner.
            $(Spinner_Loading).hide();
        });
}

/**
 * Open layout Restock.
 */
let restockStartDate = null;
function openLayoutRestock() {
    isRestock = false;
    restockStartDate = Date.now();
    console.log("restockStartDate: " + restockStartDate);
    cleanRestockData();
    showHideLayout(Footer, false);
    reloadInventory();
    setMachineStatus('RESTOCKING_PRODUCT');
    $(Table_Restock + " tbody tr").remove();
    showHideLayout(Layout_restock, true);
    showHideLayout(Layout_Option, false);
}

/**
 * Open layout Unload.
 */
function openLayoutUnload() {
    // set machine machine status to unload
    setMachineStatus('UNLOADING_PRODUCT');
    showHideLayout(Footer, false);

    showHideLayout(Layout_unload, true);
    showHideLayout(Layout_Option, false);
}

/**
 * Event comeback layout Options.
 */
function clickLayoutOptions() {
    setMachineStatus('IDLE');
    isRestock = false;
    showHideLayout(Layout_restock, false);
    showHideLayout(Layout_unload, false);
    showHideLayout(Layout_Option, true);
    showHideLayout(Layout_Result, false);

    // Remove all old data.
    $(Table_Restock + " tbody tr").remove();
    $(Table_Restock + " tfoot tr").remove();
}

function clearCartAndShowLoading() {
    clearInterval(intervalCountdown);
    lockRefreshTable = true;
    $(Spinner_Loading_Sale_Msg).text('Please wait for scanning...');
    $(Spinner_Loading_Sale).show();

    logger.info("clearCartAndShowLoading");

    $(Content_Shoppingcart).html("");
    $(Content_Shoppingcart).html("SCANNING...");
    //sleep(5000);
    cartDelayTimer = setTimeout(() => {
        logger.info("clearCartAndShowLoading after sleep");

        lockRefreshTable = false;
        enableButtons();
        countDownPayment();
        $(Spinner_Loading_Sale).hide();
    }, inventoryCartUiDelay);

}

// function sleep(ms) {

// }

/**
 * Event confirm Restock More.
 */



function clickConfirmRestockWithSession() {
    restockStartDate = Date.now();

    console.log("isRestock: " + isRestock);
    if (isRestock) {
        return;
    }
    // Start the loading spinner.
    $(Spinner_Loading).show();
    isRestock = true;
    $.when(getProducts(true)).done(function () {

        if ($(Table_Restock + '  tbody').children().length === 0 && $(Table_Unload + '  tbody').children().length === 0) {
            showMessage(false, 'No products.', Layout_restock);
            return;
        }

        if ($(Table_Restock + '  tbody tr').hasClass('item-unmapped')) {
            showMessage(false, 'There are unmapped products.', Layout_restock);
            return;
        } else {
            let resultConfirm = 'Success';

            let dataRestockDto = {
                "items": dataRestock,
                "restockerName": employee
            };
            logger.info("dataRestockDto: " + JSON.stringify(dataRestockDto));

            let dataUnloadDto = {
                "ids": dataUnload,
                "machineId": machineId,
                "restockerName": employee
            };
            logger.info("dataUnloadDto: " + JSON.stringify(dataUnloadDto));

            let dataRestockSessionDto = {
                "restock": dataRestockDto,
                "unload": dataUnloadDto,
                "restockerName": employee,
                "startDate": restockStartDate
            };

            var data = JSON.stringify(dataRestockSessionDto);
            logger.info("Confirm Restock Data: " + data);
            $.ajax({
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                async: false,
                'type': 'POST',
                'url': URL_CONFIRM_RESTOCKSESSION,
                'data': JSON.stringify(dataRestockSessionDto),
                'dataType': 'json'
            })
                // Success.
                .done((success) => {
                    logger.info("Confirm Success.");
                    if (!success.result) {
                    }

                    reloadInventory();

                })
                // Error.
                .fail((error, msg) => {
                    logger.info("Confirm Error msg." + msg);
                    logger.info("Confirm Error." + JSON.stringify(error));
                    var errMessage = error.responseJSON.error.message;
                    showMessage(false, 'Request failed: ' + msg, Layout_restock);
                    resultConfirm = 'Unsuccess - ' + errMessage;
                    return;
                })
                // finally.
                .always(() => {
                    $(Spinner_Loading).hide();
                    logger.info("Confirm Finished.");
                    if ($('#' + Layout_restock).is(':visible')) {
                        $(Title_Result).text('Restock/Unload');
                        $(Title_Button).text('Restock More');
                        $(Content_Result).text(resultConfirm);
                        isUnload = false;
                    } else {
                        $(Title_Result).text('Unload');
                        $(Title_Button).text('Unload More');
                        $(Content_Result).text(resultConfirm);
                        isUnload = true;
                    }
                    showHideLayout(Layout_Result, true);
                    showHideLayout(Layout_restock, false);
                    showHideLayout(Layout_unload, false);
                    isRestock = false;
                });
        }
    });


}



function clickConfirmRestock() {
    if (isRestock) {
        return;
    }
    // Start the loading spinner.
    $(Spinner_Loading).show();
    isRestock = true;

    // Call again api get product.
    getProducts(true);

    if ($(Table_Restock + '  tbody').children().length === 0) {
        showMessage(false, 'No products.', Layout_restock);
        return;
    }

    if ($(Table_Restock + '  tbody tr').hasClass('item-unmapped')) {
        showMessage(false, 'There are unmapped products.', Layout_restock);
        return;
    } else {
        let resultConfirm = 'Success';

        let dataRestockDto = {
            "items": dataRestock,
            "restockerName": employee
        };

        var data = JSON.stringify(dataRestockDto);
        logger.info("Confirm Restock Data: " + data);
        $.ajax({
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            'type': 'POST',
            'url': URL_CONFIRM_RESTOCK,
            'data': JSON.stringify(dataRestockDto),
            'dataType': 'json'
        })
            // Success.
            .done((success) => {
                logger.info("Confirm Success.");
                if (!success.result) {
                }

                reloadInventory();

            })
            // Error.
            .fail((error, msg) => {
                logger.info("Confirm Error msg." + msg);
                logger.info("Confirm Error." + JSON.stringify(error));
                var errMessage = error.responseJSON.error.message;
                showMessage(false, 'Request failed: ' + msg, Layout_restock);
                resultConfirm = 'Unsuccess - ' + errMessage;
                return;
            })
            // finally.
            .always(() => {
                $(Spinner_Loading).hide();
                logger.info("Confirm Finished.");
                if ($('#' + Layout_restock).is(':visible')) {
                    $(Title_Result).text('Restock');
                    $(Title_Button).text('Restock More');
                    $(Content_Result).text(resultConfirm);
                    isUnload = false;
                } else {
                    $(Title_Result).text('Unload');
                    $(Title_Button).text('Unload More');
                    $(Content_Result).text(resultConfirm);
                    isUnload = true;
                }
                showHideLayout(Layout_Result, true);
                showHideLayout(Layout_restock, false);
                showHideLayout(Layout_unload, false);
                isRestock = false;
            });
    }
}

/**
 * Event confirm Unload More.
 */
function clickConfirmUnload() {
    if (unloadLoading) {
        return;
    }
    if ($(Table_Unload + '  tbody').children().length === 0) {
        showMessage(false, 'No products.', Layout_unload);
        return;
    }
    // Start the loading spinner.
    $(Spinner_Loading).show();
    unloadLoading = true;
    let resultConfirm = 'Success';
    logger.info("clickConfirmUnload");

    logger.info("dataUnload: " + dataUnload);
    let dataUnloadDto = {
        "ids": dataUnload,
        "machineId": machineId,
        "restockerName": employee
    };
    $.ajax({
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        'type': 'POST',
        'url': URL_CONFIRM_UNLOAD,
        'data': JSON.stringify(dataUnloadDto),
        'dataType': 'json'
    })
        // Success.
        .done((result) => {
            logger.info("Confirm Success.", result, 'TrungPQ');
            reloadInventory();
        })
        // Error.
        .fail(() => {
            logger.info("Confirm Error.");
            resultConfirm = 'UnSuccess';
            return;
        })
        // finally.
        .always(() => {
            $(Spinner_Loading).hide();
            logger.info("Confirm Finished.");
            if ($('#' + Layout_restock).is(':visible')) {
                $(Title_Result).text('Restock');
                $(Title_Button).text('Restock More');
                $(Content_Result).text(resultConfirm);
                isUnload = false;
            } else {
                $(Title_Result).text('Unload');
                $(Title_Button).text('Unload More');
                $(Content_Result).text(resultConfirm);
                isUnload = true;
            }
            showHideLayout(Layout_Result, true);
            showHideLayout(Layout_restock, false);
            showHideLayout(Layout_unload, false);
            unloadLoading = false;
        });
}

/**
 * Event Unload or Restock More.
 */
function clickUnloadRestockMore() {
    reloadInventory();
    if (isUnload) {
        // set machine machine status to unload
        setMachineStatus('UNLOADING_PRODUCT');

        showHideLayout(Layout_Result, false);
        showHideLayout(Layout_restock, false);
        showHideLayout(Layout_unload, true);
    } else {
        // Remove all old data.
        $(Table_Restock + " tbody tr").remove();
        $(Table_Restock + " tfoot tr").remove();
        showHideLayout(Layout_Result, false);
        showHideLayout(Layout_restock, true);
        showHideLayout(Layout_unload, false);
    }
}

function getProducts(isConfirm = false) {

    isRestock = false;
    if (isGetProduct) {
        return;
    }
    // Start the loading spinner.
    $(Spinner_Loading).show();
    isGetProduct = true;

    logger.info("Start Get Products Success.");
    let proAndTag = [];
    let dataProduct = {
        "machineId": machineId,
        "tags": tags
    };

    var apiUrl = "/api/services/app/ProductTags/QueryProductByTag";
    if (usePrefixMethod) {
        apiUrl = "/api/services/app/ProductTags/QueryProductByTagV2";
    }

    logger.info("dataProduct:" + JSON.stringify(dataProduct));
    return $.ajax({
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        async: false,
        'type': 'POST',
        'url': cloudUrl + apiUrl,
        'data': JSON.stringify(dataProduct),
        'dataType': 'json'

    })
        // Success.
        .done((result) => {
            logger.info("Get Products Success.");
            proAndTag = result.result;
            logger.info(result.result);
        })
        // Error.
        .fail((error) => {
            logger.info("Get Products Error.");
            resultConfirm = 'Failed';
            showMessage(false, 'Can not get products.', Layout_restock);
        })
        // finally.
        .always(() => {
            // Remove all old data.
            $(Table_Restock + " tbody tr").remove();
            $(Table_Restock + " tfoot tr").remove();

            // Show product and tag to table.
            let htmlRow = '';
            dataRestock = [];
            var notMappedTag = 0;
            tags.forEach(tag => {
                let item = _.find(proAndTag, ['tag', tag]);

                if (typeof item !== typeof undefined) {
                    // htmlRow += '<tr><td>' + item.productName + '</td><td>' + item.tag + '</td><td>' + formatter.format(item.price) + '</td></tr>';
                    let itemPost = {
                        "tagId": item.tag,
                        "trayLevel": 0,
                        "price": item.price,
                        "productId": item.productId,
                        "id": ""
                    };
                    dataRestock.push(itemPost);
                    logger.info("dataRestock PUSH: " + JSON.stringify(dataRestock))

                } else {
                    notMappedTag++;
                    //htmlRow += '<tr class="item-unmapped" style="background: red;"><td>Unmapped</td><td>' + tag + '</td><td></td></tr>';
                }
            });



            let groupedproAndTag = _.chain(proAndTag)
                .groupBy("productId")
                .toPairs()
                .map(function (currentItem) {
                    return _.zipObject(["productId", "items"], currentItem);
                })
                .value();
            logger.info("groupedproAndTag: " + JSON.stringify(groupedproAndTag));
            logger.info(groupedproAndTag);

            logger.info(proAndTag);

            groupedproAndTag.forEach(item => {
                logger.info(item);
                var name = item.items[0].productName;
                var price = item.items[0].price;
                var quantity = item.items.length;

                htmlRow += '<tr><td>' + name + '</td><td>' + formatter.format(price) + '</td><td>' + quantity + '</td></tr>';
            });

            if (notMappedTag > 0) {
                htmlRow += '<tr class="item-unmapped" style="background: red;"><td>Unmapped</td><td>' + formatter.format(0) + '</td><td>' + notMappedTag + '</td></tr>';
            }


            $(Total_Items_Restock).text('Total: ' + proAndTag.length);
            $(Table_Restock + " tbody").append(htmlRow);
            $(Spinner_Loading).hide();
            isGetProduct = false;
            logger.info("Get Products Finished.");
        });
}

function processRestockData(data) {
    let htmlRow = '';

    $(Table_Unload + " tbody tr").remove();
    $(Table_Unload + " tfoot tr").remove();
    var removed = _.takeRightWhile(data, ['State', 1]);
    var unloadIds = _.map(removed, 'Id');
    dataUnload = unloadIds;


    logger.info("removed: " + JSON.stringify(removed));

    let groupedRemoved = _.chain(removed)
        .groupBy("Product.Id")
        .toPairs()
        .map(function (currentItem) {
            return _.zipObject(["productId", "items"], currentItem);
        })
        .value();

    logger.info("groupedRemoved: " + JSON.stringify(groupedRemoved));

    groupedRemoved.forEach(item => {
        var name = item.items[0].Product.ProductName;
        var price = item.items[0].Product.Price;
        var quantity = item.items.length;

        htmlRow += '<tr><td>' + name + '</td><td>' + formatter.format(price) + '</td><td>' + quantity + '</td></tr>';
    });

    $(Table_Unload + " tbody").append(htmlRow);
}


function showMessage(isSuccess, msg, element) {
    let backgroundColor = '#dc3545';
    let borderColor = '#dc3545';

    if (isSuccess) {
        backgroundColor = '#28a745';
        borderColor = '#28a745';
    }

    $('#' + element).append(`<div id="msg-false" style="
        display: inline-block;
        position: fixed;
        top: 0;
        bottom: 0;
        left: 0;
        right: 0;
        width: 100%;
        height: 5rem;
        margin: auto;
        color: #fff;
        background-color: ` + backgroundColor + `;
        border-color: ` + borderColor + `;
        border: 1px solid transparent;
        border-radius: .25rem;
        font-size: 35px;
        padding-top: 1rem;
        font-weight: 600;">
        ` + msg + `
        </div>`);
    setTimeout(() => {
        $('#msg-false').remove();
    }, 5000);
}

function countDownPayment() {
    // logger.info("=========================================================================")
    let countdown = timeWaitAutoPayment;
    clearInterval(intervalCountdown);
    intervalCountdown = setInterval(() => {
        $(Count_Down_Payment).text('Transaction will complete in ' + countdown / second + 's');
        if (countdown === 0) {
            clearInterval(intervalCountdown);
            $(Count_Down_Payment).text('');
            logger.info("Make payment by timer")
            makePayment();
        }
        // logger.info("================== " + countdown)
        countdown -= second;

    }, second);
}