# ExpensAI

## Visão Geral
ExpensAI é um **hub inteligente de finanças pessoais** que consolida gastos de cartões, apps de delivery e bancos, categoriza despesas automaticamente e fornece previsões de gastos mensais. A solução combina um backend **ASP.NET Core** para upload e dashboard, com um serviço **Python (FastAPI)** que utiliza **Pandas** e **Scikit‑learn** para análise e IA.

---

## Como rodar
### Pré‑requisitos
- **Docker Desktop** (ou Docker Engine) instalado
- **.NET SDK 6+** (se executado localmente)
- **Python 3.11** (se executado localmente)

### Passos
1. Clone o repositório
   ```bash
   git clone https://github.com/MatheusLeo26/smartflow-finance.git
   cd smartflow-finance
   ```
2. Inicie os containers
   ```bash
   docker compose up -d
   ```
   Isso cria:
   - `backend` – API ASP.NET Core
   - `ai_service` – FastAPI + Pandas/Scikit‑learn
   - `db` – Banco de dados PostgreSQL padrão configurado para simulação local de tabelas.
3. Acesse a documentação Swagger do backend .NET em `http://localhost:5000/swagger` ou chame a API Python diretamente em `http://localhost:8000/docs`.

---

## Arquitetura utilizada
```
User --> .NET Frontend (Upload) --> REST POST /process --> Python FastAPI Service
      |                                               |
      |<-- JSON de transações categorizadas -----------|
      |
      v
  Database (Users, RawFiles, Transactions, Categories, Predictions)
```
- **Comunicação**: Chamadas REST síncronas entre .NET e o endpoint FastAPI `/process`.
- **Persistência**: Banco relacional central integrado que mantém usuários, arquivos, transações categorizadas e predições históricas.
- **ML**: Modelo de regressão linear simples que gera previsões baseadas no histórico financeiro mapeado do usuário.

---

## Desafios técnicos superados
- **Integração Heterogênea .NET/Python**: Criação de canais de API robustos usando Docker Compose para permitir chamadas HTTP diretas com baixo overhead.
- **Estruturação de Modelo de Segurança Híbrido**: Arquitetura planejada com JWT + RTR, Always Encrypted para proteção de dados financeiros sensíveis em repouso e validação de Magic Numbers nos uploads de arquivos.
- **CI/CD Integrado**: Configuração de um pipeline seguro no GitHub Actions com detecção de vulnerabilidades de código de ambos os ecossistemas simultaneamente.

---

## Diferenciais de Portfólio
- **Micro-serviço Multi-linguagem** (C# e Python integrados).
- **Controle Automatizado de Dependências & Segurança** no pipeline (SAST/SCA).
- **Proposta de Criptografia Multicamadas** (mTLS + Database Encryption).