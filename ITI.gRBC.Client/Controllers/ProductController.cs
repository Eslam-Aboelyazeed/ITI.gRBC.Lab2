using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Grpc.Net.Client;
using ITI.gRBC.Client.Protos;
using static ITI.gRBC.Client.Protos.InventoryService;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ITI.gRBC.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        //public InventoryServiceClient client;

        //public ProductController()
        //{
        //    var channel = GrpcChannel.ForAddress("https://localhost:7246");

        //    client = new InventoryServiceClient(channel);
        //}

        private readonly InventoryServiceClient client;

        public ProductController(InventoryServiceClient client)
        {
            this.client = client;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddOrUpdateProduct(Product product)
        {
            var res = await client.getProductByIdAsync(new ProductId()
            {
                Id = product.Id
            });

            if (res.Flag)
            {
                var response = await client.updateProductAsync(product);

                return Ok(response);
            }
            else
            {
                var response = await client.addProductAsync(new InsertProduct()
                {
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = product.Quantity
                });

                return Ok(response);
                //return CreatedAtRoute("getProduct", new {id = response.Id},response);
            }
        }

        [HttpPost("/api/addBulkProducts")]
        public async Task<ActionResult<ProductCount>> AddBulkProducts(List<Product> products)
        {
            if (products == null || products.Count == 0)
            {
                return BadRequest();
            }

            var call = client.addBulkProducts();

            foreach (var product in products)
            {
                await call.RequestStream.WriteAsync(product);

                //await Task.Delay(1000);
            }

            await call.RequestStream.CompleteAsync();

            var productCount = await call.ResponseAsync;

            return Ok(productCount.Count);
        }

        [HttpPost("/api/generateProductReport")]
        public async Task<ActionResult<List<Product>>> GenerateProductReport(Critiria critiria)
        {
            //var productCount = await client.getProductCountAsync(new ProductCountRequest());

            var call = client.getProductReport(critiria);

            var products = new List<Product>();

            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                products.Add(call.ResponseStream.Current);
                //await Task.Delay(1000);
            }

            return Ok(products);
        }

        //{
//        [
//              {
//                "id": 1,
//                "name": "Phone1",
//                "price": 1000,
//                "quantity": 10,
//                "category": 0,
//                "expireDate": {
//                  "seconds": 0,
//                  "nanos": 0
//                }
//},
//              {
//    "id": 2,
//                "name": "Phone2",
//                "price": 1200,
//                "quantity": 15,
//                "category": 0,
//                "expireDate": {
//        "seconds": 0,
//                  "nanos": 0
//                }
//},
//              {
//    "id": 3,
//                "name": "Phone3",
//                "price": 1500,
//                "quantity": 8,
//                "category": 0,
//                "expireDate": {
//        "seconds": 0,
//                  "nanos": 0
//                }
//},
//              {
//    "id": 4,
//                "name": "Phone4",
//                "price": 2000,
//                "quantity": 5,
//                "category": 1,
//                "expireDate": {
//        "seconds": 0,
//                  "nanos": 0
//                }
//},
//              {
//    "id": 5,
//                "name": "Phone5",
//                "price": 1900,
//                "quantity": 6,
//                "category": 1,
//                "expireDate": {
//        "seconds": 0,
//                  "nanos": 0
//                }
//}
//            ]
        //}
    }
}
