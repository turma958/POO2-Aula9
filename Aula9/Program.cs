// Criar usuario cada vez com mais dependencias
// Quantidade de dpendências crescendo exponencialmente
// Arrumar um local para resolver as dependências
// Dependências aninhadas

//var connection = new FakeDB<Usuario>();
//var contaConnection = new FakeDB<Conta>();
//var settings = new CriptografiaSettings();
//var emailSettings = new EmailSettings();
//var emailService = new EmailService(emailSettings);
//var templateEmailService = new TemplateEmailService(emailService);
//var criptografia = new CriptografiaPBKDF2(settings);
//var repo = new UsuarioRepository(connection);
//var contaRepo = new ContaRepository(contaConnection);
//var useCase = new CriarUsuarioUseCase(repo, criptografia, templateEmailService, contaRepo);
//useCase.Executar(new Usuario());

// RESOLVENDOOOOOOO
// Registrar as dependências
using Microsoft.Extensions.DependencyInjection;

// Transient - instanciar um novo objeto TODA VEZ
// Scoped - instanciar o objeto apenas UMA VEZ durante o escopo de vida
// Singleton - instanciar apenas UMA VEZ sempre

var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IDbConnection<Usuario>, FakeDB<Usuario>>();
serviceCollection.AddTransient<IDbConnection<Conta>, FakeDB<Conta>>();

serviceCollection.AddTransient<IRepository<Usuario>, UsuarioRepository>();
serviceCollection.AddTransient<IContaRepository, ContaRepository>();

serviceCollection.AddTransient<ICriptografia, CriptografiaPBKDF2>();

serviceCollection.AddTransient<IEmailService, EmailService>();
serviceCollection.AddTransient<ITemplateEmailService, TemplateEmailService>();

serviceCollection.AddTransient<CriptografiaSettings>();
serviceCollection.AddTransient<EmailSettings>();

serviceCollection.AddTransient<CriarUsuarioUseCase>();

// Instanciar o injetor
var container = serviceCollection.BuildServiceProvider();

// Usar e ser feliz
var useCase = container.GetRequiredService<CriarUsuarioUseCase>();
useCase.Executar(new Usuario());

Console.ReadLine();

class Usuario
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Nome { get; set; }
    public string Password { get; set; }
}

interface IDbConnection<T>
{
    List<T> Data { get; set; }
}

class FakeDB<T> : IDbConnection<T>
{
    public List<T> Data { get; set; }
}

interface ICriptografia
{
    string Encrypt(string text);
}

interface IRepository<T>
{
    void Add(T obj);
    T Get(string id);
    IEnumerable<T> List();
    void Remove(T obj);
    void Remove(string id);
}

// NAO USEM ESSE ALGORITMO
class CriptografiaMD5 : ICriptografia
{
    // Imaginem que está criptografando
    public string Encrypt(string text) => text;
}

class CriptografiaSettings
{
    public string Key { get; set; }
    public string Salt { get; set; }
}

class CriptografiaPBKDF2 : ICriptografia
{
    private readonly CriptografiaSettings _settings;

    public CriptografiaPBKDF2(CriptografiaSettings settings)
    {
        _settings = settings;
    }

    // Imaginem que está criptografando
    public string Encrypt(string text) => text;
}

class MailMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

class EmailSettings
{
    public string Smtp { get; set; }
    public string Usuario { get; set; }
    public string Senha { get; set; }
}

interface IEmailService
{
    void Send(MailMessage mail);
}

class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(EmailSettings settings)
    {
        _settings = settings;
    }

    public void Send(MailMessage mail)
    {
        // Lógica de envio de e-mail
    }
}

interface ITemplateEmailService
{
    void EnviarEmailBoasVindas(string email, string nome);
}

class TemplateEmailService : ITemplateEmailService
{
    private readonly IEmailService _emailService;

    public TemplateEmailService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public void EnviarEmailBoasVindas(string email, string nome)
    {
        _emailService.Send(new MailMessage
        {
            To = email
        });
    }
}

class CriarUsuarioUseCase
{
    private readonly IRepository<Usuario> _repositorio;
    private readonly ICriptografia _criptografia;
    private readonly IContaRepository _contaRepository;
    private readonly ITemplateEmailService _templateEmailService;

    public CriarUsuarioUseCase(IRepository<Usuario> repositorio, 
        ICriptografia criptografia, ITemplateEmailService templateEmailService,
        IContaRepository contaRepository)
    {
        _repositorio = repositorio;
        _criptografia = criptografia;
        _contaRepository = contaRepository;
        _templateEmailService = templateEmailService;
    }

    public void Executar(Usuario usuario)
    {
        usuario.Password = _criptografia.Encrypt(usuario.Password);
        _repositorio.Add(usuario);
        _contaRepository.CreditWelcome(usuario.Id);
        _templateEmailService.EnviarEmailBoasVindas(usuario.Email, usuario.Nome);
    }
}

interface IContaRepository
{
    void CreditWelcome(string usuarioId);
}

class Conta { }

class ContaRepository : IContaRepository
{
    private readonly IDbConnection<Conta> _dbConnection;

    public ContaRepository(IDbConnection<Conta> dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public void CreditWelcome(string usuarioId)
    {
    }
}

class UsuarioRepository : IRepository<Usuario>
{
    private readonly IDbConnection<Usuario> _dbConnection;

    public UsuarioRepository(IDbConnection<Usuario> dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public void Add(Usuario obj)
    {
        _dbConnection.Data.Add(obj);
    }

    public Usuario Get(string id)
    {
        return _dbConnection.Data.FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<Usuario> List()
    {
        return _dbConnection.Data;
    }

    public void Remove(Usuario obj)
    {
        _dbConnection.Data.Remove(obj);
    }

    public void Remove(string id)
    {
        var usuario = Get(id);

        if (usuario != null)
            Remove(usuario);
    }
}