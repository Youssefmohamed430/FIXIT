#region NameSpaces
    global using FIXIT.Domain.Entities;
    global using FIXIT.Application.Servicces;
    global using FIXIT.Domain.ValueObjects;
    global using FIXIT.Application.DTOs;
    global using FIXIT.Application.IServices;
    global using FIXIT.Domain.Abstractions;
    global using FIXIT.Domain;
    global using FIXIT.Presentation.Controllers;
    global using FIXIT.Application.Handlers;
    global using FIXIT.Domain.Factories;
    global using FIXIT.Domain.Helpers;
#endregion

#region Packages
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
#endregion


global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Cryptography;
global using System.Text;
global using System.Net;
global using System.Net.Mail;
global using Mapster;
