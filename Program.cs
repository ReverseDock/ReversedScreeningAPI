using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IFASTAService, FASTAService>();
builder.Services.AddScoped<IDockingPrepService, DockingPrepService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IAlphaFoldReceptorService, AlphaFoldReceptorService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongoDB(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddAsyncAPI(builder.Configuration);
builder.Services.AddCors(c => {
    c.AddPolicy("AllowAnything", options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true));
});

var app = builder.Build();

app.UseCors("AllowAnything");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

