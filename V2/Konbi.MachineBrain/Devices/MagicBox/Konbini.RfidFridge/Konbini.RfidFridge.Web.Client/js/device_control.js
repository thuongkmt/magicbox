function OpenLock()
{
    RbmqPub("OPEN_LOCK")
}

function EnableTerminal()
{
    RbmqPub("ENABLE_TERMINAL")
}

function DisableTerminal()
{
    RbmqPub("DISABLE_TERMINAL")
}