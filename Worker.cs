// Importa namespaces necess�rios para o funcionamento do servi�o, configura��o, RabbitMQ e logging
using RabbitMQ.Client;
using System.Text;

namespace WorkService.Producer
{
    // Define uma classe Worker que herda de BackgroundService para ser executada como um servi�o em segundo plano
    public class Worker : BackgroundService
    {
        private readonly IConnection _connection; // Declara uma vari�vel para a conex�o com RabbitMQ
        private readonly IModel _channel; // Declara uma vari�vel para o canal RabbitMQ
        private readonly ILogger<Worker> _logger; // Declara uma vari�vel para o logger

        // Construtor da classe Worker que recebe um logger como par�metro
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            var connectionFactory = new ConnectionFactory // Cria uma f�brica de conex�es RabbitMQ
            {
                HostName = "localhost", // Endere�o do servidor RabbitMQ
                UserName = "user",          // Nome de usu�rio para autentica��o
                Password = "password",          // Senha para autentica��o
                Port = 5672,                // Porta do servidor RabbitMQ
                AutomaticRecoveryEnabled = true, // Habilita a recupera��o autom�tica da conex�o
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // Intervalo de recupera��o da rede
            };

            try
            {
                // Tenta criar uma conex�o com o RabbitMQ usando a f�brica de conex�es configurada
                _connection = connectionFactory.CreateConnection("workservice.producer");
                // Cria um canal de comunica��o com o RabbitMQ a partir da conex�o estabelecida
                _channel = _connection.CreateModel();
            }
            catch (Exception ex) // Captura exce��es durante a conex�o e cria��o do canal
            {
                // Loga um erro caso ocorra alguma exce��o ao tentar conectar ou criar o canal
                _logger.LogError($"Erro ao conectar no RabbitMQ: {ex.Message}");
                throw; // Lan�a a exce��o novamente ap�s logar o erro
            }
        }

        // M�todo sobreposto que executa o servi�o em segundo plano
        protected override Task ExecuteAsync(CancellationToken stoppingToken) // M�todo principal que executa o servi�o em segundo plano
        {
            var message = "Professor Hello, RabbitMQ!"; // Define a mensagem a ser enviada
            var body = Encoding.UTF8.GetBytes(message); // Converte a mensagem para um array de bytes

            // Publica a mensagem na fila "minha-fila"
            _channel.BasicPublish(exchange: "topiclog.ex", routingKey: "app1.critical", basicProperties: null, body: body);

            _logger.LogInformation($"Mensagem enviada: {message}"); // Loga a mensagem enviada

            return Task.CompletedTask; // Retorna uma tarefa conclu�da
        }

        public override void Dispose() // M�todo chamado quando o servi�o � finalizado
        {
            // Fecha o canal e a conex�o quando o servi�o for finalizado
            _channel.Close();
            _connection.Close();
            base.Dispose(); // Chama o m�todo Dispose da classe base
        }
    }
}
