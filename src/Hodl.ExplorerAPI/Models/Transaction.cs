﻿using System.Text.Json.Serialization;


namespace Hodl.ExplorerAPI.Models;

public class Transaction
{
    [JsonPropertyName("blockNumber")]
    public string BlockNumber { get; set; }

    [JsonPropertyName("timeStamp")]
    public string TimeStamp { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("nonce")]
    public string Nonce { get; set; }

    [JsonPropertyName("blockHash")]
    public string BlockHash { get; set; }

    [JsonPropertyName("transactionIndex")]
    public string TransactionIndex { get; set; }

    [JsonPropertyName("from")]
    public string From { get; set; }

    [JsonPropertyName("to")]
    public string To { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("gas")]
    public string Gas { get; set; }

    [JsonPropertyName("gasPrice")]
    public string GasPrice { get; set; }

    [JsonPropertyName("isError")]
    public string IsError { get; set; }

    [JsonPropertyName("txreceipt_status")]
    public string TxReceiptStatus { get; set; }

    [JsonPropertyName("input")]
    public string Input { get; set; }

    [JsonPropertyName("contractAddress")]
    public string ContractAddress { get; set; }

    [JsonPropertyName("cumulativeGasUsed")]
    public string CumulativeGasUsed { get; set; }

    [JsonPropertyName("gasUsed")]
    public string GasUsed { get; set; }

    [JsonPropertyName("confirmations")]
    public string Confirmations { get; set; }

    //[JsonPropertyName("tokenName")]
    //public string TokenName { get; set; }

    //[JsonPropertyName("tokenSymbol")]
    //public string TokenSymbol { get; set; }

    //[JsonPropertyName("tokenDecimal")]
    //public string TokenDecimal { get; set; }

}