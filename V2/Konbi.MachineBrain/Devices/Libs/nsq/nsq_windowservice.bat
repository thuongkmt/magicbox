sc create nsqlookupd binpath= "c:\nsq\nsqlookupd.exe" start= auto DisplayName= "nsqlookupd"
sc description nsqlookupd "nsqlookupd 0.3.2"
sc start nsqlookupd

sc create nsqd binpath= "c:\nsq\nsqd.exe -mem-queue-size=0 -lookupd-tcp-address=127.0.0.1:4160 -data-path=c:\nsq\data" start= auto DisplayName= "nsqd"
sc description nsqd "nsqd 0.3.2"
sc start nsqd