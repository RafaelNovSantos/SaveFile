using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

class Program
{
    static string lastMessage = "";
    
    static void Main()
    {
        string configFilePath = "config.xml";  // Arquivo XML de configuração
        string logFilePath = "log.txt";  // Caminho para o arquivo de log

        // Pega o nome do processo atual automaticamente
        string processName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
        
       

        while (true)
        {
            // Verifica e encerra qualquer processo anterior
            TerminateExistingProcess(processName);

            // Lê as configurações do arquivo XML
            (string sourceFilePath, string destinationDirectory) = ReadConfig(configFilePath);

            if (sourceFilePath == null || destinationDirectory == null)
            {
                LogMessage(logFilePath, "Configuração inválida. Criando um novo arquivo de configuração.");
                CreateDefaultConfig(configFilePath); // Cria um novo arquivo de configuração
                continue;
            }

            // Verifica se o arquivo de origem existe
            if (File.Exists(sourceFilePath))
            {
                string destinationFilePath = Path.Combine(destinationDirectory, Path.GetFileName(sourceFilePath));

                try
                {
                    // Verifica se o diretório de destino existe
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory); // Cria o diretório de destino, se não existir
                    }

                    if (!File.Exists(destinationFilePath)){
                       
                        string successMessage = $"Arquivo copiado para: {destinationFilePath}";
                        Console.WriteLine(successMessage);
                        LogMessage(logFilePath, successMessage);
                    }
                    
                    // Copia o arquivo
                    File.Copy(sourceFilePath, destinationFilePath, true);
                    
                    /*string infoMessage = $"Arquivo reescrito: {destinationFilePath}";
                    Console.WriteLine(infoMessage);
                    LogMessage(logFilePath, infoMessage);*/
                    

                    
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Erro ao copiar o arquivo: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    LogMessage(logFilePath, errorMessage);
                }
            }
            else
            {
                if (lastMessage != "Arquivo não encontrado.")
                {
                    
                
                string notFoundMessage = "Arquivo não encontrado.";
                Console.WriteLine(notFoundMessage);
                LogMessage(logFilePath, notFoundMessage);
                }
            }

            Thread.Sleep(5000); // Espera 5 segundos antes de verificar novamente
        }
    }

    // Função para encerrar um processo existente, mas não o próprio
    static void TerminateExistingProcess(string processName)
    {
        try
        {
            // Obtém todos os processos em execução com o nome especificado
            var processes = Process.GetProcessesByName(processName);

            // Se houver processos em execução
            foreach (var process in processes)
            {
                // Não tenta finalizar o próprio processo
                if (process.Id != Process.GetCurrentProcess().Id)
                {
                    // Tente matar o processo e aguard'e a finalização
                    Console.WriteLine($"Tentando encerrar o processo {processName} (PID: {process.Id})...");
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Processo {processName} encerrado com sucesso.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao tentar encerrar o processo {processName}: {ex.Message}");
        }
    }

    static (string sourceFilePath, string destinationDirectory) ReadConfig(string configFilePath)
    {
        try
        {
            if (File.Exists(configFilePath))
            {
                // Garante que o arquivo está em UTF-8
                EnsureUtf8Encoding(configFilePath);

                // Usando StreamReader para garantir a leitura com codificação UTF-8
                using (StreamReader reader = new StreamReader(configFilePath, Encoding.UTF8))
                {
                    string configContent = reader.ReadToEnd();

                    // Lê o conteúdo como um XML
                    XDocument config = XDocument.Parse(configContent);

                    // Lê os valores do XML
                    string sourceFilePath = config.Root.Element("localDoArquivo")?.Value;
                    string destinationDirectory = config.Root.Element("localDeSalvamento")?.Value;

                    return (sourceFilePath, destinationDirectory);
                }
            }
            else
            {
                string errorMessage = "Arquivo de configuração não encontrado.";
                Console.WriteLine(errorMessage);
                LogMessage("log.txt", errorMessage);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro ao ler o arquivo de configuração: {ex.Message}";
            Console.WriteLine(errorMessage);
            LogMessage("log.txt", errorMessage);
        }

        return (null, null);
    }

    static void EnsureUtf8Encoding(string filePath)
    {
        try
        {
            // Lê o conteúdo do arquivo com a codificação atual
            string content = File.ReadAllText(filePath, Encoding.Default);

            // Reescreve o arquivo com codificação UTF-8 sem BOM
            File.WriteAllText(filePath, content, new UTF8Encoding(false)); // false para garantir sem BOM
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro ao garantir codificação UTF-8: {ex.Message}";
            Console.WriteLine(errorMessage);
            LogMessage("log.txt", errorMessage);
        }
    }

    static void CreateDefaultConfig(string configFilePath)
    {
        try
        {
            // Cria um conteúdo XML padrão
            XDocument defaultConfig = new XDocument(
                new XElement("configuracao",
                    new XElement("localDoArquivo", @"caminho do arquivo que será copiado"),
                    new XElement("localDeSalvamento", @"caminho de salvamento do arquivo")
                )
            );

            // Adiciona a declaração XML com UTF-8
            defaultConfig.Declaration = new XDeclaration("1.0", "UTF-8", null);

            // Salva o arquivo com a codificação UTF-8 sem BOM
            defaultConfig.Save(configFilePath);
            string successMessage = $"Novo arquivo de configuração criado: {configFilePath}";
            Console.WriteLine(successMessage);
            LogMessage("log.txt", successMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro ao criar o arquivo de configuração: {ex.Message}";
            Console.WriteLine(errorMessage);
            LogMessage("log.txt", errorMessage);
        }
    }

    static void LogMessage(string logFilePath, string message)
    {
        try
        {
             lastMessage = message;
            // Escreve no arquivo de log com timestamp
            using (StreamWriter writer = new StreamWriter(logFilePath, append: true, encoding: Encoding.UTF8))
            {
                writer.WriteLine($"[{DateTime.Now}] {message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar o log: {ex.Message}");
        }
    }
}
