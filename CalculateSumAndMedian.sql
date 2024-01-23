CREATE DEFINER=`root`@`localhost` PROCEDURE `CalculateSumAndMedian`()
BEGIN
    DECLARE totalSum BIGINT;
    DECLARE decimalMedian DECIMAL(18, 8);
SELECT 
    SUM(IntegerNumber)
INTO totalSum FROM
    Files;
SELECT 
    AVG(DecimalNumber)
INTO decimalMedian FROM
    (SELECT 
        DecimalNumber,
            @rownum:=@rownum + 1 AS rownum,
            @totalrows:=@rownum
    FROM
        Files, (SELECT @rownum:=0) r
    ORDER BY DecimalNumber) AS subquery
WHERE
    rownum IN (FLOOR((@totalrows + 1) / 2) , FLOOR((@totalrows + 2) / 2));

    -- Выводим результаты
    SELECT totalSum AS TotalSum, decimalMedian AS DecimalMedian;
END