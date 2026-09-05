Ui.montarLayout('vendas');

const elLista = document.getElementById('lista');
let vendasCache = [];

async function carregar() {
  Ui.loading(elLista);
  try {
    vendasCache = await Api.get('/api/vendas');
    render(vendasCache);
  } catch (e) {
    Ui.erro(e.message);
    Ui.vazio(elLista, 'Não foi possível carregar as vendas.', '⚠️');
  }
}

function render(vendas) {
  if (!vendas || vendas.length === 0) { Ui.vazio(elLista, 'Nenhuma venda registrada ainda.', '🛍️'); return; }
  elLista.innerHTML = `
    <div class="tabela-wrap">
      <table class="tabela">
        <thead>
          <tr>
            <th>Nº</th><th>Data</th><th>Cliente</th><th>Itens</th>
            <th class="num">Total</th><th>Status</th><th class="acoes">Ações</th>
          </tr>
        </thead>
        <tbody>
          ${vendas.map(v => `
            <tr>
              <td>#${v.id}</td>
              <td>${Ui.data(v.dataVenda)}</td>
              <td>${Ui.escape(v.clienteNome || 'Consumidor não identificado')}</td>
              <td>${v.itens.length}</td>
              <td class="num">${Ui.moeda(v.total)}</td>
              <td>${Ui.badge(v.statusTexto)}</td>
              <td class="acoes">
                <button class="btn-icon" title="Ver detalhes" data-ver="${v.id}">👁️</button>
                <button class="btn-icon" title="Cancelar venda" data-cancelar="${v.id}" ${v.status === 2 ? 'disabled' : ''}>❌</button>
              </td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
  elLista.querySelectorAll('[data-ver]').forEach(b => b.addEventListener('click', () => verDetalhes(Number(b.dataset.ver))));
  elLista.querySelectorAll('[data-cancelar]').forEach(b => b.addEventListener('click', () => cancelar(b.dataset.cancelar)));
}

function verDetalhes(id) {
  const v = vendasCache.find(x => x.id === id);
  if (!v) return;
  const corpo = `
    <p><strong>Cliente:</strong> ${Ui.escape(v.clienteNome || 'Consumidor não identificado')}</p>
    <p><strong>Data:</strong> ${Ui.data(v.dataVenda)} &nbsp;·&nbsp; <strong>Status:</strong> ${Ui.badge(v.statusTexto)}</p>
    <div class="tabela-wrap" style="margin-top:12px;">
      <table class="tabela">
        <thead><tr><th>Produto</th><th class="num">Preço</th></tr></thead>
        <tbody>
          ${v.itens.map(i => `<tr><td>${Ui.escape(i.produtoNome)}</td><td class="num">${Ui.moeda(i.precoUnitario)}</td></tr>`).join('')}
          <tr><td><strong>Total</strong></td><td class="num"><strong>${Ui.moeda(v.total)}</strong></td></tr>
        </tbody>
      </table>
    </div>`;
  Ui.abrirModal(`Venda #${v.id}`, corpo);
}

async function cancelar(id) {
  const ok = await Ui.confirmar('Cancelar esta venda? Os produtos voltarão ao estoque.', 'Cancelar venda');
  if (!ok) return;
  try {
    const r = await Api.patch(`/api/vendas/${id}/cancelar`, {});
    Ui.sucesso(r.mensagem || 'Venda cancelada.');
    carregar();
  } catch (e) { Ui.erro(e.message); }
}

carregar();
