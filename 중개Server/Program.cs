using Microsoft.EntityFrameworkCore;
using 재무인사Common.Model;

var builder = WebApplication.CreateBuilder(args);

// 데이터베이스 컨텍스트 및 데이터 시더 서비스 추가
// 데이터베이스 컨텍스트를 환경에 따라 다르게 설정
if (builder.Environment.IsDevelopment())
{
    // 개발 환경: InMemory 데이터베이스 사용
    builder.Services.AddDbContext<재무인사DbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb"));
}
else
{
    // 프로덕션 환경: SQL Server 데이터베이스 사용
    builder.Services.AddDbContext<재무인사DbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
builder.Services.AddScoped<DataSeeder>();

// 기타 서비스 구성
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 데이터베이스 초기화를 위한 로직 추가
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var seeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataSeeder>();
    var filePath = Path.Combine(app.Environment.ContentRootPath, "Setup", "은행코드.xlsx");
    seeder.SeedDataAsync(filePath).Wait();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
