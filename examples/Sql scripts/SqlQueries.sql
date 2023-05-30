-------------------------------------------------------------------------------
-- Check size of the database
-------------------------------------------------------------------------------
SELECT SUM(pg_database_size(pg_database.datname)) / (1024 * 1024) as size_mb 
FROM pg_database;

select table_schema, table_name, 
       pg_size_pretty(pg_total_relation_size('"'||table_schema||'"."'||table_name||'"')) as size
from information_schema.tables;

-------------------------------------------------------------------------------
-- Truncate changelog and vacuum
-------------------------------------------------------------------------------
truncate table public."ChangeLog";

VACUUM (VERBOSE, ANALYZE);


-------------------------------------------------------------------------------
-- Custom or imported listings for period
-------------------------------------------------------------------------------
SELECT T2."Symbol", T1.*
	
FROM public."Listings" as T1
JOIN public."CryptoCurrencies" as T2 ON T1."CryptoId" = T2."Id"
	
	WHERE "Source" not in ('CoinGecko', 'CoinMarketCap')
	AND   "TimeStamp" > '2020-12-01'
	AND   "TimeStamp" < '2021-01-01'
	
ORDER BY T2."Symbol", T1."TimeStamp";


-------------------------------------------------------------------------------
-- Fund holdings in period
-------------------------------------------------------------------------------
SELECT T1."Id", T1."CurrencyISOCode", T2."Symbol", T1."BookingPeriod", T1."StartBalance", T1."StartUSDPrice", T1."EndBalance", T1."EndUSDPrice", T1."NavBalance"
	FROM public."Holdings" AS T1
	LEFT OUTER JOIN public."CryptoCurrencies" AS T2 on T1."CryptoId" = T2."Id"
	WHERE "FundId" = '43bbbdd0-8ae6-431b-ba8f-b43e7f9c23e1'
	-- WHERE "FundId" = 'fb4ef97b-7541-4e2c-83b9-2989dfb0e309'
	AND   "BookingPeriod" = '202105'
	
ORDER BY T1."CurrencyISOCode", T2."Symbol";


-------------------------------------------------------------------------------
-- Select NAV's
-------------------------------------------------------------------------------
SELECT "Id", "FundId", "Type", "DateTime", "BookingPeriod", "TotalValue", "TotalShares", "ShareHWM", "ShareGross", "ShareNAV", "CurrencyRateId", "Date", "AdministrationFee", "PerformanceFee", "InOutShares", "InOutValue"
	FROM public."Navs"
	ORDER BY "BookingPeriod", "FundId", "DateTime";


-------------------------------------------------------------------------------
-- Select order with multiple fee currencies
-------------------------------------------------------------------------------
SELECT "OrderNumber"
FROM   public."Orders"
WHERE  "Id" IN (
	SELECT "OrderId"
	FROM (
		SELECT DISTINCT "OrderId", "FeeCurrencyId"
		FROM   public."Trades"
	) AS t1
	GROUP BY "OrderId"
	HAVING COUNT("FeeCurrencyId") > 1
)






-- Temp
SELECT "Id", "HoldingId", "OppositeTransferId", "BookingPeriod", "DateTime", "TransactionType", "TransactionSource", "TransactionId", "Direction", "TransferAmount", "TransferFee", "Reference", "Shares", "FeeHoldingId"
	FROM public."Transfers"
	-- WHERE "TransactionType" in ('AdministrationFee', 'PerformanceFee');
	WHERE "TransactionType" = 'Inflow' AND "TransferAmount" = 0;
	
DELETE FROM public."Holdings"
WHERE "Id" = 'cbbe8ab1-97ff-4fa8-8a17-6a7352bad440';