namespace Hodl.ExplorerAPI.Configurations;

public class SubScanOptions
{
    public string ApiKey { get; set; }
    //  https://support.subscan.io/#getting-start:~:text=Every%20user%20could%20get%20a%20free%20plan%20for%20a%20trial.%20Free%20plan%20has%20a%20rate%20limit%20of%205%20req/s%20and%20can%20only%20generate%201%20API%20Key.
    public int CallsPerSecond { get; set; } = 5;
    // https://subscan.medium.com/api-service-price-adjustment-announcement-2c6a582ac4eb#:~:text=Free%20Plan%20will%20have%20100%2C000%20req/d%20limit.
    public int CallsPerDay { get; set; } = 100000;
}
