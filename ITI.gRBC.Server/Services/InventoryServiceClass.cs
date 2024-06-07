using Grpc.Core;
using ITI.gRBC.Server.Protos;
using Microsoft.AspNetCore.Authorization;
using static ITI.gRBC.Server.Protos.InventoryService;

namespace ITI.gRBC.Server.Services
{
    [Authorize(AuthenticationSchemes = Consts.ApiKeySchemeName)]
    public class InventoryServiceClass: InventoryServiceBase
    {
        public static List<Product> Products = new List<Product>();

        public override async Task<ProductBooleanResponse> getProductById(ProductId request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(p => p.Id == request.Id);

            if (product != null)
            {
                return await Task.FromResult(new ProductBooleanResponse()
                {
                    Flag = true
                });
            }

            return await Task.FromResult(new ProductBooleanResponse()
            {
                Flag = false
            });
        }

        public override async Task<Product> addProduct(InsertProduct request, ServerCallContext context)
        {
            var id = Products.OrderByDescending(p => p.Id).FirstOrDefault()?.Id + 1 ?? 1;

            var product = new Product()
            {
                Id = id,
                Name = request.Name,
                Price = request.Price,
                Quantity = request.Quantity
            };

            Products.Add(product);

            return await Task.FromResult(product);
        }

        public override async Task<Product> updateProduct(Product request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(p => p.Id == request.Id);


            product.Name = request.Name; 
            product.Price = request.Price;
            product.Quantity = request.Quantity;

            return await Task.FromResult(product);
        }

        public override async Task<ProductCount> getProductCount(ProductCountRequest request, ServerCallContext context)
        {
            return await Task.FromResult(new ProductCount()
            {
                Count = Products.Count
            });
        }

        public override async Task<ProductCount> addBulkProducts(IAsyncStreamReader<Product> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                Products.Add(request);
            }

            return await Task.FromResult(new ProductCount() { Count = Products.Count});
        }

        public override async Task<Task> getProductReport(Critiria request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
        {
            var products = Products.AsQueryable();

            if (request.IsOrderByPrice)
            {
                products = products.OrderBy(p => p.Price);
            }

            if (request.IsGroupedByCategory)
            {
                var productsGroup = Products.GroupBy(p => p.Category);

                var productList = new List<Product>();

                foreach (var group in productsGroup)
                {
                    foreach (var item in group)
                    {
                        productList.Add(item);
                    }
                }

                products = productList.AsQueryable();
            }

            try
            {
                var categoryFilter = (int)request.CategoryFilter;

                if (categoryFilter < 3 && categoryFilter >= 0)
                {
                    products = products?.Where(p => p.Category == request.CategoryFilter);
                }
            }
            catch (Exception) { }

            //while (!context.CancellationToken.IsCancellationRequested)
            //{
            //    Task.Run(async () =>
            //    {
            //        foreach (var product in products)
            //        {
            //            await responseStream.WriteAsync(product);

            //            await Task.Delay(1000);
            //        }
            //        return Task.CompletedTask;
            //    });
            //}

            foreach (var product in products)
            {
                await Task.Run(async () =>
                {
                    await responseStream.WriteAsync(product);

                });
            }

            //await Task.Delay(1000);

            return Task.CompletedTask;
        }
    }
}
