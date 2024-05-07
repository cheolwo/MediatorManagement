using Microsoft.EntityFrameworkCore;
using �繫�λ�Common.Model;

var builder = WebApplication.CreateBuilder(args);

// �����ͺ��̽� ���ؽ�Ʈ �� ������ �ô� ���� �߰�
// �����ͺ��̽� ���ؽ�Ʈ�� ȯ�濡 ���� �ٸ��� ����
if (builder.Environment.IsDevelopment())
{
    // ���� ȯ��: InMemory �����ͺ��̽� ���
    builder.Services.AddDbContext<�繫�λ�DbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDb"));
}
else
{
    // ���δ��� ȯ��: SQL Server �����ͺ��̽� ���
    builder.Services.AddDbContext<�繫�λ�DbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
builder.Services.AddScoped<DataSeeder>();

// ��Ÿ ���� ����
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// �����ͺ��̽� �ʱ�ȭ�� ���� ���� �߰�
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var seeder = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataSeeder>();
    var filePath = Path.Combine(app.Environment.ContentRootPath, "Setup", "�����ڵ�.xlsx");
    seeder.SeedDataAsync(filePath).Wait();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
