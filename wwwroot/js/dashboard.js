Ui.montarLayout('dashboard');

async function carregar() {
  try {
    const d = await Api.get('/api/dashboard');
    renderStats(d);
    renderUltimasVendas(d.ultimasVendas);
    renderProdutosRecentes(d.produtosRecentes);
  } catch (e) {
    document.getElementById('stats').innerHTML = '';
    Ui.erro(e.message || 'Não foi possível carregar o dashboard.');
  }
}

function renderStats(d) {
  const cards = [
    { label: 'Total de produtos', valor: d.totalProdutos },
    { label: 'Disponíveis', valor: d.produtosDisponiveis },
    { label: 'Vendidos', valor: d.produtosVendidos },
    { label: 'Total de vendas', valor: d.totalVendas },
    { label: 'Faturamento total', valor: Ui.moeda(d.faturamentoTotal), destaque: true },
  ];
  document.getElementById('stats').innerHTML = cards.map(c => `
    <div class="stat ${c.destaque ? 'stat--destaque' : ''}">
      <div class="stat__label">${c.label}</div>
      <div class="stat__valor">${c.valor}</div>
    </div>`).join('');
}

function renderUltimasVendas(vendas) {
  const el = document.getElementById('ultimas-vendas');
  if (!vendas || vendas.length === 0) { Ui.vazio(el, 'Nenhuma venda registrada ainda.', '🧾'); return; }
  el.innerHTML = `
    <div class="tabela-wrap">
      <table class="tabela">
        <thead><tr><th>Cliente</th><th>Data</th><th class="num">Total</th></tr></thead>
        <tbody>
          ${vendas.map(v => `
            <tr>
              <td>${Ui.escape(v.cliente)}</td>
              <td>${Ui.data(v.data)}</td>
              <td class="num">${Ui.moeda(v.total)}</td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
}

function renderProdutosRecentes(produtos) {
  const el = document.getElementById('produtos-recentes');
  if (!produtos || produtos.length === 0) { Ui.vazio(el, 'Nenhum produto cadastrado ainda.', '👕'); return; }
  el.innerHTML = `<ul class="lista-simples">
    ${produtos.map(p => `
      <li>
        <span>${Ui.escape(p.nome)} <span class="muted">· ${Ui.escape(p.categoria)}</span></span>
        <strong>${Ui.moeda(p.preco)}</strong>
      </li>`).join('')}
  </ul>`;
}

carregar();
