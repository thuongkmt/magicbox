function customerStart() {
    logger.info("Customer Start");
    clearInterval(intervalCountdown);
    disableButtons();
    $.post("http://localhost:9000/api/machine/customer/action/pressstart", {}, (result) => { })
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
