# MCP Employee Server - HTTP

A hands-on learning project demonstrating how to build an **MCP (Model Context Protocol) Server using .NET 8 and C#** with HTTP transport.

This project is part of my practical learning journey into MCP, AI Agents, LLM tool calling, and enterprise AI architecture.

## Overview

The application exposes employee-related functionality as MCP tools.

An MCP client can connect to the server over HTTP, discover the available tools, and invoke them using the MCP protocol.

### Architecture

```text
                    MCP Client
                        |
                        | HTTP
                        v
              +---------------------+
              | MCP Employee Server |
              |       .NET 8        |
              +---------------------+
                        |
                        v
                 Employee Tools
                        |
                        v
                      Data


```HTTP MCP Flow
                +-------------+
                | MCP Client  |
                +-------------+
                    |
                    | HTTP
                    v
                +----------------------+
                | /mcp endpoint        |
                | MCP HTTP Transport   |
                +----------------------+
                    |
                    v
                +----------------------+
                | MCP Server           |
                +----------------------+
                    |
                    v
                +----------------------+
                | EmployeeTools        |
                +----------------------+
                    |
                    v
                +----------------------+
                | Employee Data        |
                +----------------------+