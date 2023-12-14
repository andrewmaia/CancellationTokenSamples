# CancellationToken - O que você precisa e deveria saber antes de usar


No mundo moderno da engenharia de software os processos , atividades e dependências ocorrem a todo o momento de forma **concorrente**!

Falar de **concorrência** é ter consciência que várias tarefas são executadas simultaneamente e/ou concorrentes com os ciclos/processadores disponíveis e que estes **recursos são finitos e/ou tem custos associados à sua utilização.**

>  Obter performance é bem mais sobre **saber trabalhar** com **concorrência** e como **usar de forma consciente os recursos** que sobre saber aplicar padrões arquiteturais.

De um modo geral (minha opinião) **falhamos miseravelmente em criar aplicações / bibliotecas e funcionalidades que fazem bom uso de recursos!**

**A "nuvem" até pode ser “infinita”, mas certamente o cartão de crédito e sua infraestrutura NÃO!**=

> **CancellationToken** é um componente desenhado para **informar** que não é mais necessário continuar uma tarefa, fornecendo um mecanismo para cancelamento cooperativo de operações assíncrona e **propagando** a informação de solicitação de cancelamento por todas as tarefas que a utilizam.

**Alguns aspectos importantes:**

> O cancelamento **não é imposto/forçado é uma solicitação!** As tarefas **podem e devem** determinar como e quando encerrar em resposta a uma solicitação de cancelamento.

**Não existe mágica**, o **CancellationToken** não faz nada além de informar uma solicitação de cancelamento, _isso é muito importante._

>  A solicitação de **cancelamento refere-se sempre a operações e não a objetos**, em outras palavras, devemos criar “**operações canceláveis**” para poder tirar proveito do CancellationToken.

O **CancellationToken** informa, e você precisa e deve saber o que fazer com esta solicitação utilizando **componentes adequados**, _isso é ainda mais importante!_

> **Operações canceláveis** são tarefas que escolhem uma **estratégia de como encerrar e como responder** a uma solicitação de cancelamento. Normalmente, executam alguma limpeza necessária e respondem o mais **breve possível**.

**Novamente não existe mágica**, o que precisa ser feito após o recebimento da solicitação de cancelamento deve estar bem definido e **alinhado com as regras de negócio e os componentes utilizados**.

Além disso a implementação **devem ter suporte para este tipo de operação**. _Isso é mais que importante é fundamental!_

**_Mas_ como sei que foi enviado solicitação de cancelamento?**

> **_Através da propriedade IsCancellationRequested_**

**Duas notas antes de continuar**:

* Esta repo demostra apenas o **CancellationToken,** o uso de tarefas assíncronas (Async / await) não estão dentro deste escopo.
  
* Todos os exemplos apresentados são **apenas didáticos e necessariamente não representam as melhores prática**.
  

Tornando sua Api “Cancelável”
-----------------------------

Quando uma solicitação de “request” é enviada para sua api, é mantida uma conexão entre as duas partes envolvidas (quem está consumindo / quem está processando a solicitação).

**O que acontece se a parte que solicitou abandonar a solicitação?** A conexão é perdida e o processamento **continua na sua api**! (desperdício de processamento , recursos de Infraestrutura/Midllewares e os custos associados : financeiros e performance).  

Toda vez que é estabelecida uma conexão, “por baixo dos panos” é disponibilizado um parâmetro de **CancellationToken (opcional – deve ser declarado)** em sua solicitação associado à esta conexão.

Quando ocorre uma “quebra” de conexão entre as partes é enviado uma solicitação de cancelamento (**IsCancellationRequested = true**) para que você tenha a **oportunidade de tomar uma ação** para não continuar a operação, uma vez que a outra parte não tem mais interesse e nem ira receber o resultado da operação.

> Dica : Sempre **declarar e propagar** o **CancellationToken** para as **chamadas subjacentes**, oferecendo a **oportunidade** as demais dependências de tomar uma ação para não continuar a operação o mais **breve possível**.

O .NET já fornece diversos métodos associados a cada recurso que já estão preparados para cancelar a operação caso recebam uma notificação de cancelamento.

**_Lembre-se: A solicitação de cancelamento refere-se sempre a operações e não a objetos, então o fato de seu recurso(objeto) de api ou tarefa/função(objeto) ser ou não “Async” é irrelevante!_**

**_ATENÇÃO:_** _Normalmente os métodos associados a cada recurso que aceitam o cancelamento esperam uma_ **_operação assíncrona_** _criando esta relação com “Async/await”, então ao executar_ **_uma operação assíncrona sobre uma operação síncrona_**_, embora seja possível, deve ser_ **_feita com muito critério!_**

* * *

**Exemplo: Acessando outra dependência de api (Tarefa não cancelável) e sem token:**

_Para testar use a_ **_aplicação “CancellationTokenApiSample”_** _e chame o ‘Endpoint’_ **_“Call/HelloWord”_** _, em seguida_ **_execute um refresh no browser; esta ação interrompe a conexão e ativa a solicitação de cancelamento_**_._

Neste exemplo é feito uma chamada a outro “Endpoint” (que demora 10 segundos para recuperar a informação) . Como não tem a declaração de cancelamento e a dependência também **não é uma tarefa cancelável** a execução continua **até o final na api e na dependência sem ser interrompida.** (Veja os tempos)

* * *

**Exemplo: Acessando outra dependência de api (Tarefa não cancelável) e com CancellationToken:**

_Para testar use a_ **_aplicação “CancellationTokenApiSample”_** _e chame o ‘Endpoint’_ **_ “CallCT /HelloWord’_** _em seguida_ **_execute um refresh no browser;_**

Como tem a declaração de cancelamento  e a dependência **não é uma tarefa cancelável** a execução **continua até o final na dependência** sendo **interrompida na api**. (Veja os tempos)

* * *

**Exemplo: Acessando outra dependência de api (Tarefa cancelável) e com CancellationToken:**

_Para testar use a_ **_aplicação “CancellationTokenApiSample”_** _e chame o ‘Endpoint’_ **_“CallCT /_**  **_HelloWordCancelation”_** _em seguida_ **_execute um refresh no browser;_**

Como tem a declaração de cancelamento  e a dependência **é uma tarefa cancelável** a execução é **interrompida na api e na dependência.** (Veja os tempos)

* * *

**Exemplo: Acessando um banco de dados (Tarefa não cancelável) e sem CancellationToken:**

_Para testar use a_ **_aplicação “CancellationTokenApiSample”_** _e chame o ‘Endpoint’_ **_“Call/OpenDb”_** _em seguida_ **_execute um refresh no browser;_**

Neste exemplo e feito um acesso ao um banco de dados abrindo e fechando a conexão (Para simular um atraso, a conexão aponta para um endereço “errado” para tentar 6 vezes).  Quando ocorre o a solicitação de cancelamento a **consulta ao banco continua** .(veja o tempo)

* * *

**Exemplo: Acessando um banco de dados (Tarefa cancelável) e com CancellationToken:**

_Para testar use a_ **_aplicação “CancellationTokenApiSample”_** _e chame o ‘Endpoint’_ **_“CallCT/OpenDb”_** _em seguida_ **_execute um refresh no browser;_**

Neste exemplo e feito um acesso ao um banco de dados abrindo e fechando a conexão. Quando ocorre o a solicitação de cancelamento a **consulta ao banco é interrompida** .(veja o tempo)

Tornando uma Tarefa/Função com temporização “Cancelável”
--------------------------------------------------------

Nos exemplos anteriores é feito chamadas a dois “Endpoint” diferentes

* **/HelloWord**
    
* **/HelloWordCancelation**
    

O “Endpoint” **HelloWord é uma tarefa não cancelável**

O “Endpoint” **HelloWordCancelation é uma tarefa cancelável.**

> **Atenção: não é porque recebe o parâmetro CancellationToken se torna cancelável!**

Comparando os dois códigos, você pode notar que existem algumas diferenças:

* Recebe um parâmetro **CancellationToken** (Propagação! lembra da dica no início?)
    
* Aproveita o Token para executar o Delay:  ‘**token.WaitHandle.WaitOne**(10000);’ (Vamos falar disso agora)
    

Lembra da definição de uma operação cancelável ? Uma ajudinha :

> **operações canceláveis** São tarefa que escolhem uma **estratégia de como encerrar e como responder** a uma solicitação de cancelamento. Normalmente, executam alguma limpeza necessária e respondem o mais **breve possível**.

O código original usa o comando **‘Thread.Sleep(10000)’**. Este comando interrompe a execução Thread por um tempo determinado para simular um trabalhado demorado/ custoso de um recurso. 

Queremos que  isso aconteça, porém somente quando **não houver** uma solicitação de cancelamento, então precisamos de uma **outra estratégia** para fazer isso e responder o **mais breve possível** quando ocorre o cancelamento.

A propriedade ‘**WaitHandle’** disponibiliza o manipulador do token que é responsável por controlar a sincronização entre threads e aplicar os sinalizadores (semáforos) para que isso aconteça.

O Legal da  propriedade ‘**WaitHandle’** é que possui alguns métodos para interromper a execução de forma temporizada. Quando ocorre uma solicitação de cancelamento esta **temporização é interrompida** e “voilà”!  temos agora uma estratégia eficiente e rápida!

> Dica : Utilize sempre que possível **WaitHandle**  e seus métodos como  sinalizadores de tempo(WaitOne ou seus semelhantes).  Quando ocorre uma solicitação de cancelamento seus métodos também são avisados e **interrompem** a espera/ação programada

Tornando uma tarefa em background “Cancelável” em um host web
-------------------------------------------------------------

Uma tarefa em “background” normalmente é iniciada junto com aplicação e permanece ativa durante o ciclo de vida dela, sobre um “loop” baseado em uma condição/regra qualquer.

Em cenários atuais, onde “container é vida” e os conceitos de “**HPA’  (“upscale” / ”downscale”) são necessários para fazem uso consciente de recursos** as tarefas em “background” precisam **saber a hora de parar de forma controlada** evitando “travamentos” e/ou **inconsistências** , fazendo as limpezas necessárias durante este processo quando necessário. Isso é válido mesmo sem o uso de “container”.

O uso de **CancellationToken** é um bom “aliado” para garantir uma saída deste “loop” de uma forma controlada (“**Graceful Shutdown”**).

* * *

**Exemplo: Tarefa em background (Tarefa cancelável) e com CancellationToken:**

_Para testar use a_ **_aplicação “CancellationTokenBackGroudServiceSample”_** _e acompanhe o log por cerca de 30 segundos e depois chame o ‘Endpoint’ “_**_StopApplication_**_” ;_ **_esta ação envia uma solicitação para finalizar a aplicação que por sua vez dispara uma solicitação de cancelamento._**

Analisando o código, você poderá ver um loop temporizado pela classe ‘**PeriodicTimer’**. Antes de executar a ação desejada é feita uma validação de negócio ‘AnyRule’ que quando atendida executa uma **tarefa cancelável** ‘DoWork’.  Os resultados podem ser vistos e avaliados pelo log. Quando chega a solicitação de cancelamento o "loop" **encerra de forma controlada (Graceful Shutdown).**

Uma boa prática observada ,como já dito, é a **propagação** do **CancellationToken** para as dependências subjacentes , que no nosso caso é o método ‘DoWork’.

Outra observação é o uso do comando ‘**ThrowIfCancellationRequested()**’ que notifica ao chamador uma solicitação de cancelamento abortando (neste exemplo faz sentido) a execução.

> **ThrowIfCancellationRequested** : Gera um **OperationCanceledException** se esse token tiver tido o cancelamento solicitado.


Assumindo o controle do “Cancelamento”
--------------------------------------

Sempre temos disponível pela Infraestrutura do .NET o **CancellationToken** de **encerramento da aplicação**, porém  outras  necessidades aparecem em cenários que queremos cancelar alguma tarefa com **base em um evento, estado ou regra de negócio**. Sendo assim precisamos **assumir o controle**! Nesta hora entra em cena outro componente:

> **CancellationTokenSource**:  É uma classe desenhada para ser uma fonte de notificação de cancelamento controlada pela aplicação.

Adivinha qual é a principal propriedade desta classe? O **CancellationToken!**

_Nota: Esta classe implementa a interface_ **_IDisposable_** _então lembre-se de utilizar o método_ **_Dispose()_** _quando não for mais necessário ou_  _encapsular sua utilização em um_ **_bloco de “using”._**

O **CancellationTokenSource** possui  alguns métodos para atender ao seu propósito e vamos abordar alguns deles (Existem vários outros métodos, abordaremos outros adiante)

* **Cancel**
    
* **CancelAfter**
    
* **Register**
    
* * *

### O método Cancel

**O que faz o método Cancel?** Comunica uma solicitação de cancelamento, em outras palavras, tornam a propriedade **IsCancellationRequested = true!**


**Exemplo: CancellationTokenSource – Método Cancel**

_Para testar use a_ **_aplicação “CancellationTokenCancelConsole”_** _, siga as instruções que apareceram no console e acompanhe o log._

Neste exemplo temos um serviço em background que inicia duas tarefas assíncronas:

·        **TaskUI:** Responsável  pela interação do console para enviar um comando de processar mensagens  ou uma solicitação de cancelamento e encerrar a aplicação.

·        **TaskProcess:** Responsável  por processar uma mensagem.

Analisando o código , você pode observar o uso do **CancellationTokenSource** com o método **Cancel**  na ‘**TaskUI**’ para enviar uma solicitação de cancelamento para Task ‘**TaskProcess**’; **Propagação**!

Nota : Existe um problema nesta abordagem; Quando ocorre um **Ctrl-C** a Task ‘**TaskProcess**’ **não é encerrada automaticamente** pois ele apenas observa o **CancellationTokenSource** sendo  preciso  enviar outro **Cancel** durante o método **‘StopAsync’** e aguarda o término da Task para termos uma saída controlada (**Graceful Shutdown**) e um **fluxo correto de sua lógica**(executar o comando 'applicationLifetime.StopApplication()' após o loop).

* * *

### O método CancelAfter

**O que faz o método CancelAfter(TimeSpan)/ CancelAfter(Int32)?** Comunica uma solicitação de cancelamento **após o número especificado de tempo**.

Um exemplo prático para este método é o uso em regras de **TIMEOUT** para  execuções de **qualquer atividade.**

**Exemplo: CancellationTokenSource – Método CancelAfter**

_Para testar use a_ **_aplicação “CancellationTokenCancelAfterConsole”_** _siga as instruções que apareceram no console e acompanhe o log._

Neste exemplo temos um serviço em background que inicia uma tarefa assíncrona que chama outra **tarefa cancelável** para processar a mensagem com um “**flag**” para ficar em espera  **além do tempo máximo definido** ,demostrando um controle de timeout (após um **tempo pré-definido** a tarefa é **cancelada)**.

Observe que o parâmetro de **CancellationToken** é um **CancellationTokenSource** que foi usado o método **CancelAfter(10000)** antes da chamada.

* * *

### O método Register

**O que faz o método Register()?** Registra um **‘action’** que será chamado quando este **CancellationToken** for cancelado.

_Se este token já estiver no estado cancelado, a action será executado imediatamente e de_ **_forma síncrona_**_. Qualquer exceção gerada pelo action será propagada a partir desta chamada de método._

**Exemplo: CancellationTokenSource – Método Register**

_Para testar use a_ **_aplicação “CancellationTokenRegisterConsole”_** _siga as instruções que apareceram no console e acompanhe o log._

Neste exemplo temos um serviço em background que inicia uma tarefa assíncrona que processa a mensagem. Antes de iniciar o “loop”  é feito um **registro de uma ‘action’ para ser executada  quando ocorrer o cancelamento.**

Esta ‘action’ executa o fim da aplicação que por sua vez envia uma solicitação de cancelamento e encerra o “loop”.

Tornando uma tarefa “Cancelável” associada a uma ou mais regras de negócio
--------------------------------------------------------------------------

Nem tudo é tão simples e muita das vezes precisamos implementar regras mais complexas que requer **mais de uma única fonte de cancelamento**. Vamos a um cenário real:

Você possui um método que já recebe um **CancellationToken** vindo de um “request” . Agora imagina que este método além de interromper quando a conexão é perdida interrompa também quando ultrapasse um tempo limite ou ainda melhor, quando uma regra de negócio não seja atendida durante seu processamento, o que acontecer primeiro.Nesta hora entra em cena outra classe :

> **CreateLinkedToken:**  É uma classe desenhada para ser uma **fonte de notificação de cancelamento que unifica vários tokens de cancelamento em uma única solicitação de cancelamento** .

**_Nota1:_** _Esta classe implementa a interface_ **_IDisposable_** _então lembre-se de utilizar o método_ **_Dispose()_**  quando não for mais necessário ou  encapsular sua utilização em um **_bloco de “using”._**

**_Nota2:_** Esta classe **simplifica a codificação** (basta verificar a propriedade **IsCancellationRequested** independente que que a originou), porém **continua sendo possível saber qual token de cancelamento foi a origem** (caso seja preciso tomar ações diferentes para tokens distintos).

* * *

**Exemplo: Acessando um Endpoint(Tarefa cancelável com CreateLinkedToken):**

_Para testar use a_ **_aplicação “CancellationTokenApiCreateLinkedToken”_** _e chame o ‘Endpoint’ **“Call/HelloWordWithTimeout?timeout=true”** e aguarde o resultado (será um timeout com status 499). Em seguida chame o mesmo ‘Endpoint’ e **_execute um refresh no browser; esta ação interrompe a conexão e ativa a solicitação de cancelamento.** (Acompanhe log)

Neste exemplo é feito uma chamada a outro “Endpoint” (que demora 10 segundos para recuperar a informação ) . antes da execução  é criando um **CancellationTokenSource**  e um **CreateLinkedTokenSource**  utilizando o **CancellationToken do request e o CancellationTokenSource.  **

É  simulado uma **espera maior de que o tempo máximo definido** para demostrar o  timeout.

Observe que o código faz isso utilizando **CreateLinkedTokenSource!**  Isso significa que caso **ocorra um cancelamento pelo cliente ou um timeout** a tarefa será **cancelada**!

Tornando os retornos de um API “Cancelável” rastreáveis
-------------------------------------------------------

Para finalizar vou falar de outro **aspecto fundamental** **e muito relevante** que **propositalmente** não citei no início do artigo

> Dica: Mantenha sempre os retornos coerentes de sua API quando ocorre um cancelamento por parte do cliente.

**Mas porque isso é importante? Foi dito que quando ocorreu cancelamento o cliente não tem mais interesse , e não recebe o resultado...**

**Não é apenas cancelar e se preocupar em performance e custo é fazer bom uso de recursos existentes também!** vamos detalhar isso melhor criando um cenário:

_"Você trabalha em uma empresa de comércio eletrônico/banco que possui um gateway interno para fazer a comunicações entre as diversas apis. Além de rotear estas comunicações é gerado um log de infraestrutura com os tempos e status de cada requisição para uma análise pelo time de produtos e estratégia da empresa."_

Quando ocorre um cancelamento por parte do cliente **se você não escolher um status correto** as **_métricas de status ficarão “poluídas”**,  podendo induzir a análise não precisa e **"enganosa"** mesmo que **funcionalmente não tenha erros!.**

* * *

**Exemplo: Acessando um Endpoint(Tarefa cancelável com CreateLinkedToken):**

_A aplicação_ **_“CancellationTokenApiCreateLinkedToken”_** faz este tratamento retornando um status **não padrão** (499 – Mesmo do nginx) .

Analisando o código, você vai observar que quando ocorre o cancelamento o status de retorno é 499 para cancelamento, 200 para tudo ok . 204 para timeout , 500 para um erro e outro status caso Endpoint "tenha um novo status não mapeado - falha de comunicação/documentação entre times."

**_Nota :_** _Para verificar que isso acontece de fato é necessário colocar um_ **_breakpoint_** _e acompanhar a execução._

Conclusão
---------

**CancellationToken** é um  instrumento **poderoso** para otimizar **o uso de recursos concorrentes** , tornando sua aplicação muito mais **resiliente e rápida, aproveita corretamente os recursos de Infraestrutura/Midllewares e com custos menores.**

Existem muitos outros cenários (e métodos) que podem ser explorados , mas tornaria bem mais extenso este artigo que o desejado.

Espero que com este “overview” e com os códigos disponibilizados no projeto exemplo,  você possa aproveitar melhor o **CancellationToken!**

Referências
-----------

**CancellationToken:**

[**https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken?view=net-8.0**](\\"https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken?view=net-8.0\\")

**CancellationTokenSource:**

[**https://learn.microsoft.com/pt-br/dotnet/api/system.threading.cancellationtokensource?view=net-8.0**](\\"https://learn.microsoft.com/pt-br/dotnet/api/system.threading.cancellationtokensource?view=net-8.0\\")


