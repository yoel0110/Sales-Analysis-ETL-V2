-- ================================
-- Create User-defined Table Type
-- ================================
 
 SELECT * FROM FactTable;


USE [olap_ventas]
GO

CREATE TYPE dbo.TVP_FactTable AS TABLE
(
    OrderId     INT NOT NULL,
    CustomerId  INT NOT NULL,
    ProductId   INT NOT NULL,
    OrderDate   DATE NOT NULL,
    Quantity    INT NOT NULL,
    TotalPrice  DECIMAL(12,2) NOT NULL
);
GO


CREATE PROCEDURE dbo.usp_InsertFactTable
(
    @FactTableTVP dbo.TVP_FactTable READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ExpectedRows BIGINT;
    DECLARE @InsertedRows BIGINT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @ExpectedRows = COUNT_BIG(*)
        FROM @FactTableTVP;

        INSERT INTO dbo.FactTable
        (
            OrderId,
            CustomerDimId,
            ProductDimId,
            DateDimId,
            Quantity,
            TotalPrice
        )
        SELECT
            fact.OrderId,
            customer.CustomerDimId,
            product.ProductDimId,
            dateDim.DateDimId,
            fact.Quantity,
            fact.TotalPrice
        FROM @FactTableTVP AS fact
        INNER JOIN dbo.CustomerDim AS customer
            ON customer.CustomerId = fact.CustomerId
        INNER JOIN dbo.ProductDim AS product
            ON product.ProductId = fact.ProductId
        INNER JOIN dbo.DateDim AS dateDim
            ON dateDim.Fecha = fact.OrderDate;

        SET @InsertedRows = @@ROWCOUNT;

        IF @InsertedRows <> @ExpectedRows
        BEGIN
            THROW 50001,
                'No se cargaron todos los registros. Existen claves dimensionales sin correspondencia.',
                1;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO