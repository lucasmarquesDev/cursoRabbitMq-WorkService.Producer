// Importa namespaces necessários para o funcionamento do serviço, configuração, RabbitMQ e logging
using RabbitMQ.Client;
using System.Text;

namespace WorkService.Producer
{
    // Define uma classe Worker que herda de BackgroundService para ser executada como um serviço em segundo plano
    public class Worker : BackgroundService
    {
        private readonly IConnection _connection; // Declara uma variável para a conexão com RabbitMQ
        private readonly IModel _channel; // Declara uma variável para o canal RabbitMQ
        private readonly ILogger<Worker> _logger; // Declara uma variável para o logger

        // Construtor da classe Worker que recebe um logger como parâmetro
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            var connectionFactory = new ConnectionFactory // Cria uma fábrica de conexões RabbitMQ
            {
                HostName = "localhost", // Endereço do servidor RabbitMQ
                UserName = "user",          // Nome de usuário para autenticação
                Password = "password",          // Senha para autenticação
                Port = 5672,                // Porta do servidor RabbitMQ
                AutomaticRecoveryEnabled = true, // Habilita a recuperação automática da conexão
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // Intervalo de recuperação da rede
            };

            try
            {
                // Tenta criar uma conexão com o RabbitMQ usando a fábrica de conexões configurada
                _connection = connectionFactory.CreateConnection("workservice.producer");
                // Cria um canal de comunicação com o RabbitMQ a partir da conexão estabelecida
                _channel = _connection.CreateModel();
            }
            catch (Exception ex) // Captura exceções durante a conexão e criação do canal
            {
                // Loga um erro caso ocorra alguma exceção ao tentar conectar ou criar o canal
                _logger.LogError($"Erro ao conectar no RabbitMQ: {ex.Message}");
                throw; // Lança a exceção novamente após logar o erro
            }
        }

        // Método sobreposto que executa o serviço em segundo plano
        protected override Task ExecuteAsync(CancellationToken stoppingToken) // Método principal que executa o serviço em segundo plano
        {
            var message = "Professor Hello, RabbitMQ!"; // Define a mensagem a ser enviada
            var body = Encoding.UTF8.GetBytes(message); // Converte a mensagem para um array de bytes

            // Publica a mensagem na fila "minha-fila"
            _channel.BasicPublish(exchange: "topiclog.ex", routingKey: "app1.critical", basicProperties: null, body: body);

            _logger.LogInformation($"Mensagem enviada: {message}"); // Loga a mensagem enviada

            return Task.CompletedTask; // Retorna uma tarefa concluída
        }

        public override void Dispose() // Método chamado quando o serviço é finalizado
        {
            // Fecha o canal e a conexão quando o serviço for finalizado
            _channel.Close();
            _connection.Close();
            base.Dispose(); // Chama o método Dispose da classe base
        }
    }
}
