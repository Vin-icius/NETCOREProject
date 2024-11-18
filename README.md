# NETCOREProject
Projeto acadêmico realizado para atestar o conhecimento adquirido semestralmente.

Objetivo:
Com o VisualStudio, crie uma aplicação ASP.NET Core API para uma aplicação estilo NETFLIX.

Requisitos técnicos:
1 – (0.5 ponto) Middleware de Log Serilog:
•	O middleware Serilog deve ser adicionado para gerenciamento de logs.
•	Deve ser configurado para armazenar os logs em um arquivo local com retenção de 2 dias.
•	Erros capturados pelo bloco catch dos tratamentos de exceções também devem ser registrados no log.
2 – (1.5 ponto) Autenticação por JWT – JSON WEB TOKEN:
•	Proteger a API usando JWT.
•	Prepare a API para apenas suportar tokens JWT gerados com a chave secreta da aplicação.
3 - Utilização de Classes de Serviço, ViewModels e Entidades:
•	A aplicação deve ser organizada com classes de serviço, view models e entidades para uma melhor estruturação do código. Analise com critério, quais recursos são necessários.
4 - Injeção de Dependência
•	Utilize injeção de dependência sempre que possível. Recomenda-se utilizar em serviços, repositórios e outros componentes para promover maior modularidade e flexibilidade no código.
5 - Documente a API com o Swagger (TODOS OS ENDPOINTS).





Requisitos da API:
Além dos requisitos técnicos acima, a API deve implementar os seguintes endpoints:
(0.5 ponto) POST /usuarios (requisição anônima)
•	Este endpoint tem como objetivo cadastrar um novo usuário.
•	No corpo da requisição, é necessário fornecer o nome, senha, confirmação de senha e e-mail.
•	A senha e a confirmação de senha devem coincidir; caso contrário, a requisição deverá ser rejeitada com uma mensagem explicativa.
•	Se o e-mail já estiver registrado, a requisição também deve ser rejeitada, retornando uma mensagem informando o motivo.
•	Se todos os dados estiverem corretos, o usuário deve ser salvo e seu ID retornado na resposta.
(2.5 pontos) POST /usuarios/validar (requisição anônima)
•	Este endpoint tem como objetivo validar um usuário com base no e-mail e senha fornecidos.
•	O corpo da requisição, é necessário fornecer conter o e-mail e a senha do usuário.
•	Se o corpo da requisição estiver vazio, a requisição deve ser rejeitada com uma mensagem apropriada.
•	Caso as credenciais (e-mail + senha) não correspondam a um usuário existente, rejeite a requisição e retorne uma mensagem explicando o erro.
•	Se o usuário for validado corretamente, gere um token JWT que deverá ser utilizado para autenticação em requisições autenticadas.
•	O E-mail e  Id do usuário deverá estar dentro do token.
(0.5 ponto) GET /perfis/{usuarioId} (requisição autenticada)
•	O objetivo deste endpoint é retornar todos os perfis associados ao usuário.
•	Cada perfil possui um nome e um tipo (normal ou infantil).
•	O Tipo deverá ser um ENUM.
•	A resposta deve incluir todos os perfis vinculados ao usuário.
(2 ponto) POST /perfis/{usuarioId} (requisição autenticada)
•	No corpo da requisição, devem ser enviados o nome do perfil e seu tipo (normal ou infantil).
•	Caso já exista um perfil com o mesmo nome, a requisição deverá ser rejeitada com uma mensagem informando o motivo da recusa.
•	Outra regra é o limite de 4 perfis por usuário. Caso o perfil ultrapassar o limite a requisição deve ser rejeitada com uma mensagem informando o motivo da recusa.
(1.5 ponto) PATCH /perfis/{usuarioId}/{perfilId} (requisição autenticada)
•	Este endpoint é utilizado para atualizar apenas o nome de um perfil.
•	O corpo da requisição deve incluir o novo nome do perfil.
•	Caso já exista outro perfil com o mesmo nome (exceto o perfil sendo alterado), a requisição deve ser rejeitada, retornando uma mensagem informando o motivo da recusa.
(1 ponto) GET /categorias/{query} (requisição autenticada)
•	Este endpoint tem como objetivo retornar as categorias dos conteúdos disponíveis.
•	As categorias podem ser filtradas pelo parâmetro query, se fornecido.
•	A resposta deve incluir todas as categorias encontradas.
 
Requisitos para correção da prova:
Para a correção da prova, a aplicação deverá estar compilando corretamente e os endpoints devem funcionar conforme descrito nos requisitos. As rotas devem ser exatamente com descrito acima. O código-fonte será avaliado para verificar se os requisitos foram implementados tecnicamente de forma correta. Controllers esperados: 
•	UsuariosController 
•	PerfisController
•	CategoriasController

Utilize o modelo ER a seguir para detalhes sobre os dados:
![image](https://github.com/user-attachments/assets/5ca0f2bc-4ae0-4eb2-ad02-bac1ad0ff3d0)
