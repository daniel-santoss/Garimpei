Ui.montarLayout('produtos');

const params = new URLSearchParams(location.search);
const id = params.get('id');
const editando = !!id;

const form = document.getElementById('form');
const btnSalvar = document.getElementById('btn-salvar');
const campos = {
  nome: document.getElementById('nome'),
  descricao: document.getElementById('descricao'),
  categoriaId: document.getElementById('categoriaId'),
  estado: document.getElementById('estado'),
  tamanho: document.getElementById('tamanho'),
  cor: document.getElementById('cor'),
  preco: document.getElementById('preco'),
};

if (editando) document.getElementById('titulo').textContent = 'Editar produto';

async function init() {
  await carregarCategorias();
  if (editando) await carregarProduto();
}

async function carregarCategorias() {
  try {
    const cats = await Api.get('/api/categorias');
    campos.categoriaId.insertAdjacentHTML('beforeend',
      cats.map(c => `<option value="${c.id}">${Ui.escape(c.nome)}</option>`).join(''));
  } catch (e) { Ui.erro('Não foi possível carregar as categorias.'); }
}

async function carregarProduto() {
  try {
    const p = await Api.get('/api/produtos/' + id);
    campos.nome.value = p.nome;
    campos.descricao.value = p.descricao || '';
    campos.categoriaId.value = p.categoriaId;
    campos.estado.value = p.estado;
    campos.tamanho.value = p.tamanho || '';
    campos.cor.value = p.cor || '';
    campos.preco.value = p.preco;
  } catch (e) {
    Ui.erro(e.message);
    setTimeout(() => location.href = '/pages/produtos.html', 1200);
  }
}

function limparErros() {
  form.querySelectorAll('.erro-inline').forEach(s => s.textContent = '');
  form.querySelectorAll('.invalido').forEach(i => i.classList.remove('invalido'));
}
function marcarErro(campo, msg) {
  campos[campo].classList.add('invalido');
  const s = form.querySelector(`[data-erro="${campo}"]`);
  if (s) s.textContent = msg;
}

function validar() {
  limparErros();
  let ok = true;
  if (!campos.nome.value.trim()) { marcarErro('nome', 'O nome é obrigatório.'); ok = false; }
  if (!campos.categoriaId.value) { marcarErro('categoriaId', 'Selecione uma categoria.'); ok = false; }
  if (!campos.estado.value) { marcarErro('estado', 'Selecione o estado.'); ok = false; }
  const preco = parseFloat(campos.preco.value);
  if (isNaN(preco) || preco <= 0) { marcarErro('preco', 'Preço deve ser maior que zero.'); ok = false; }
  return ok;
}

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  if (!validar()) return;

  const dto = {
    nome: campos.nome.value.trim(),
    descricao: campos.descricao.value.trim() || null,
    categoriaId: Number(campos.categoriaId.value),
    tamanho: campos.tamanho.value.trim() || null,
    cor: campos.cor.value.trim() || null,
    preco: parseFloat(campos.preco.value),
    estado: Number(campos.estado.value),
  };

  btnSalvar.disabled = true;
  btnSalvar.textContent = 'Salvando...';
  try {
    if (editando) await Api.put('/api/produtos/' + id, dto);
    else await Api.post('/api/produtos', dto);
    Ui.sucesso('Produto salvo com sucesso.');
    setTimeout(() => location.href = '/pages/produtos.html', 700);
  } catch (err) {
    Ui.erro(err.message);
    btnSalvar.disabled = false;
    btnSalvar.textContent = 'Salvar';
  }
});

init();
