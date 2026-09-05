Ui.montarLayout('vendas');

const selCliente = document.getElementById('cliente');
const selProduto = document.getElementById('produto');
const btnAdd = document.getElementById('btn-add');
const btnConfirmar = document.getElementById('btn-confirmar');
const elCarrinho = document.getElementById('carrinho');
const elTotal = document.getElementById('total');

let disponiveis = [];   // produtos disponíveis (para o select)
let carrinho = [];      // itens escolhidos { id, nome, preco }

async function init() {
  await Promise.all([carregarClientes(), carregarProdutos()]);
}

async function carregarClientes() {
  try {
    const clientes = await Api.get('/api/clientes');
    selCliente.insertAdjacentHTML('beforeend',
      clientes.map(c => `<option value="${c.id}">${Ui.escape(c.nome)}</option>`).join(''));
  } catch { Ui.erro('Não foi possível carregar os clientes.'); }
}

async function carregarProdutos() {
  try {
    disponiveis = await Api.get('/api/produtos?status=1');
    preencherSelectProdutos();
  } catch { Ui.erro('Não foi possível carregar os produtos disponíveis.'); }
}

function preencherSelectProdutos() {
  const noCarrinho = new Set(carrinho.map(i => i.id));
  const opcoes = disponiveis
    .filter(p => !noCarrinho.has(p.id))
    .map(p => `<option value="${p.id}">${Ui.escape(p.nome)} — ${Ui.moeda(p.preco)}</option>`)
    .join('');
  selProduto.innerHTML = '<option value="">Selecione um produto...</option>' + opcoes;
}

btnAdd.addEventListener('click', () => {
  const id = Number(selProduto.value);
  if (!id) { Ui.erro('Selecione um produto.'); return; }
  const p = disponiveis.find(x => x.id === id);
  if (!p) return;
  carrinho.push({ id: p.id, nome: p.nome, preco: p.preco });
  preencherSelectProdutos();
  renderCarrinho();
});

function renderCarrinho() {
  if (carrinho.length === 0) {
    elCarrinho.innerHTML = '<div class="estado-vazio"><span class="emoji">🛒</span>Nenhum item adicionado.</div>';
  } else {
    elCarrinho.innerHTML = `<ul class="lista-simples">
      ${carrinho.map((i, idx) => `
        <li>
          <span>${Ui.escape(i.nome)}</span>
          <span style="display:flex; align-items:center; gap:12px;">
            <strong>${Ui.moeda(i.preco)}</strong>
            <button class="btn-icon" title="Remover" data-rem="${idx}">🗑️</button>
          </span>
        </li>`).join('')}
    </ul>`;
    elCarrinho.querySelectorAll('[data-rem]').forEach(b =>
      b.addEventListener('click', () => { carrinho.splice(Number(b.dataset.rem), 1); preencherSelectProdutos(); renderCarrinho(); }));
  }
  const total = carrinho.reduce((s, i) => s + Number(i.preco), 0);
  elTotal.textContent = Ui.moeda(total);
  btnConfirmar.disabled = carrinho.length === 0;
}

btnConfirmar.addEventListener('click', async () => {
  if (carrinho.length === 0) return;
  const dto = {
    clienteId: selCliente.value ? Number(selCliente.value) : null,
    produtoIds: carrinho.map(i => i.id),
  };
  btnConfirmar.disabled = true;
  btnConfirmar.textContent = 'Registrando...';
  try {
    await Api.post('/api/vendas', dto);
    Ui.sucesso('Venda registrada!');
    setTimeout(() => location.href = '/pages/vendas.html', 700);
  } catch (e) {
    Ui.erro(e.message);
    btnConfirmar.disabled = false;
    btnConfirmar.textContent = 'Confirmar venda';
    // Se algum produto ficou indisponível, recarrega a lista de disponíveis
    await carregarProdutos();
  }
});

init();
