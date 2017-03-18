# UpdateStock
The program will create a LOG folder in the root directory where it is placed. It will create there a log file, atime-stamped documentation of events relevant in UpdateStock. 
## Main(string[] args)
Set the start of the program

## List<String> createListArticules(String connStringMySql, String path)
Return the list of articles (id) contens in 'InventarioTabla' table.
- connStringMysql -> String with the MySql connection.
- path -> String with the path where the program is placed.

## List<String> updateInventoryTable(String connStringMySql, String connStringSqlServer, List<String> list, String path)
Update 'InventarioTabla' table with the stock from MySql table Articles and return the articles(id) who had diferents values.
- connStringMysql -> String with the MySql connection.
- connStringSqlServer -> String with the Sql Server connection.
- list -> List<String> with the id Articles to Update.
- path -> String with the path where the program is placed.

## DataTable createListIdProducts(String connStringMySql, List<String> listUpdate, String path)
Return a DataTable with the name of the product, id_product and id_product_attribute of all articles that need to update Stock in Prestashop 'ps_stock_available' table.
- connStringSqlServer -> String with the Sql Server connection.
- listUpdate -> List<String> that is created in updateInvetoryTable.
- path -> String with the path where the program is placed.

## void updateStock(String connStringMySql, DataTable table, String path)
Update column 'quantity' (Stock) in Prestashop 'ps_stock_available' table.
- connStringSqlServer -> String with the Sql Server connection.
- table -> DataTable that is created in createListIdProducts.
- path -> String with the path where the program is placed.

