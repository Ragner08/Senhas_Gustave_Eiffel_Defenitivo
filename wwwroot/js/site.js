// Senhas de Almoço EPGE - JavaScript

// Este ficheiro contém funções e comportamentos adicionais para melhorar a experiência do utilizador.
// Estas instruções são executadas no browser, ou seja, no lado do cliente, e ajudam a tornar a aplicação mais interativa.
// O código é ativado quando a página termina de carregar, para garantir que todos os elementos HTML já existem na página.

// Este bloco esconde automaticamente os alertas visíveis após alguns segundos.
// Serve para limpar mensagens de sucesso ou erro da interface, sem precisar de o utilizador fechar manualmente.
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const closeButton = alert.querySelector('.btn-close');
            if (closeButton) {
                closeButton.click();
            }
        }, 5000);
    });
});

// Este bloco pede confirmação antes de submeter formulários que eliminam dados.
// É útil para evitar ações acidentais, como apagar uma marcação ou outro registo importante.
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('.confirm-action').forEach(function(button) {
        button.addEventListener('click', function(event) {
            const form = this.closest('form');
            if (!form) {
                return;
            }

            const message = this.getAttribute('data-confirm-message') || 'Tens a certeza que queres continuar?';
            if (!window.confirm(message)) {
                event.preventDefault();
                event.stopPropagation();
                return;
            }

            form.submit();
        });
    });
});

// Este bloco cria um efeito visual nos dias do calendário quando o cursor passa por cima.
// O objetivo é dar um feedback imediato ao utilizador, fazendo o dia parecer mais destacado.
document.addEventListener('DOMContentLoaded', function() {
    const calendarDays = document.querySelectorAll('.calendar-day');
    calendarDays.forEach(function(day) {
        day.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.05)';
            this.style.zIndex = '10';
        });
        day.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
            this.style.zIndex = '1';
        });
    });
});

// Este bloco faz scroll suave para links internos da página.
// Quando o utilizador clica num link que aponta para uma âncora, a página desloca-se de forma mais elegante.
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
});

// Este bloco melhora a validação dos formulários antes de estes serem enviados ao servidor.
// Se algum campo obrigatório estiver vazio ou inválido, o formulário é impedido de avançar.
document.addEventListener('DOMContentLoaded', function() {
    const forms = document.querySelectorAll('form');
    forms.forEach(function(form) {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
});

// Este bloco formata automaticamente os valores introduzidos em campos de moeda.
// Se o utilizador escrever um número, o sistema ajusta-o para duas casas decimais ao sair do campo.
document.addEventListener('DOMContentLoaded', function() {
    const currencyInputs = document.querySelectorAll('input[type="number"][step="0.01"]');
    currencyInputs.forEach(function(input) {
        input.addEventListener('blur', function() {
            const value = parseFloat(this.value);
            if (!isNaN(value)) {
                this.value = value.toFixed(2);
            }
        });
    });
});

// Este bloco inicializa os tooltips do Bootstrap quando existirem elementos com esse atributo.
// Os tooltips mostram pequenas mensagens informativas quando o utilizador passa o rato por cima de um elemento.
document.addEventListener('DOMContentLoaded', function() {
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltipTriggerList.forEach(function(tooltipTriggerEl) {
            new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
});

// Este bloco dá um efeito visual às linhas das tabelas quando o cursor passa por cima.
// Ajuda a melhorar a leitura dos dados e a destacar a linha selecionada.
document.addEventListener('DOMContentLoaded', function() {
    const tableRows = document.querySelectorAll('.table-hover tbody tr');
    tableRows.forEach(function(row) {
        row.addEventListener('mouseenter', function() {
            this.style.backgroundColor = 'rgba(255, 107, 53, 0.05)';
        });
        row.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '';
        });
    });
});

// Esta função abre a janela de impressão do navegador para imprimir relatórios ou páginas de dados.
function printReport() {
    window.print();
}

// Esta função exporta uma tabela para um ficheiro CSV.
// É útil para guardar dados em formato de folha de cálculo, por exemplo para relatórios.
function exportToCSV(tableId, filename) {
    const table = document.getElementById(tableId);
    if (!table) return;

    let csv = [];
    const rows = table.querySelectorAll('tr');

    rows.forEach(function(row) {
        let rowData = [];
        const cells = row.querySelectorAll('td, th');
        cells.forEach(function(cell) {
            rowData.push('"' + cell.innerText.replace(/"/g, '""') + '"');
        });
        csv.push(rowData.join(','));
    });

    const csvContent = '\uFEFF' + csv.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename + '.csv';
    link.click();
}

// Esta função permite filtrar as linhas de uma tabela conforme o texto introduzido numa caixa de pesquisa.
// É muito útil para encontrar rapidamente informação em listas longas.
function filterTable(inputId, tableId) {
    const input = document.getElementById(inputId);
    const table = document.getElementById(tableId);
    if (!input || !table) return;

    input.addEventListener('keyup', function() {
        const filter = this.value.toLowerCase();
        const rows = table.querySelectorAll('tbody tr');

        rows.forEach(function(row) {
            const text = row.innerText.toLowerCase();
            row.style.display = text.includes(filter) ? '' : 'none';
        });
    });
}

// Nota: Validação de datas no passado removida por pedido do utilizador.
// A validação final permanece no servidor; deixamos controlo do lado-cliente menos intrusivo.

// Este bloco mostra um indicador de carregamento quando um formulário está a ser submetido.
// É uma forma de informar o utilizador que a ação ainda está a ser processada.
document.addEventListener('DOMContentLoaded', function() {
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(function(button) {
        button.addEventListener('click', function() {
            const form = this.closest('form');
            if (form && form.checkValidity()) {
                this.disabled = true;
                this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>A processar...';
                form.submit();
            }
        });
    });
});

// Esta função copia texto para a área de transferência do utilizador.
// Pode ser usada para facilitar a cópia de dados como códigos ou informação importante.
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function() {
        alert('Copiado para a área de transferência: ' + text);
    }).catch(function(err) {
        console.error('Erro ao copiar: ', err);
    });
}

// Esta função mostra ou esconde a palavra-passe num campo de texto.
// É útil em formulários onde o utilizador quer verificar o que escreveu.
function togglePasswordVisibility(inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        input.type = input.type === 'password' ? 'text' : 'password';
    }
}

// Esta função calcula o total de um valor multiplicado por uma quantidade.
// É usada para facilitar cálculos simples em formulários ou interfaces de negócio.
function calculateTotal(price, quantity) {
    return (price * quantity).toFixed(2);
}

// Esta função formata valores em euros para apresentação ao utilizador.
// Usa o formato de moeda português e permite mostrar valores de forma mais clara.
function formatCurrency(amount) {
    return new Intl.NumberFormat('pt-PT', {
        style: 'currency',
        currency: 'EUR'
    }).format(amount);
}

// Esta função atualiza o saldo visível da carteira em todos os elementos que utilizam a classe wallet-balance.
// Assim, quando o saldo muda, a interface é atualizada automaticamente.
function updateWalletBalance(newBalance) {
    const walletElements = document.querySelectorAll('.wallet-balance');
    walletElements.forEach(function(element) {
        element.textContent = formatCurrency(newBalance);
    });
}

// Esta função pede confirmação antes de cancelar uma marcação.
// Ajuda a evitar cancelamentos acidentais de reservas importantes.
function confirmCancellation(bookingId) {
    return confirm('Tens certeza que deseja cancelar esta marcação? O valor será reembolsado à sua carteira.');
}

// Este bloco faz um controlo simples antes de submeter uma marcação, deixando a validação final ao servidor.
// Serve como uma primeira verificação no browser, sem substituir a validação da aplicação.
document.addEventListener('DOMContentLoaded', function() {
    const bookingForms = document.querySelectorAll('form[action*="BookMeal"]');
    bookingForms.forEach(function(form) {
        form.addEventListener('submit', function(e) {
            const dateInput = form.querySelector('input[name="date"]');
            if (dateInput) {
                console.log('Booking for date:', dateInput.value);
            }
        });
    });
});
