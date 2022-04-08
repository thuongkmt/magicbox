function GetConfig(key, callback) {
    $.ajax({
        type: 'GET',
        url: 'http://localhost:9000/api/machine/config/' + key,
        success: callback    
      });
}

