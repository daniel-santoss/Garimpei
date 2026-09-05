Ui.montarLayout('clientes');

const params = new URLSearchParams(location.search);
const id = params.get('id');
const editando = !!id;

const form = document.getElementById('form');
const btnSalvar = document.getElementById('btn-salvar');
const campos = {
  nome: document.getElementById('nome'),
  telefone: document.getElementById('telefone'),
  email: document.getElementById('email'),
};

if (editando) document.getElementById('titulo').textContent = 'Editar cliente';

async function carregar() {
  if (!editando) return;
  try {
    const c = await Api.get('/api/clientes/' + id);
    campos.nome.value = c.nome;
    campos.telefone.value = c.telefone || '';
    campos.email.value = c.email || '';
  } catch (e) {
    Ui.erro(e.message);
    setTimeout(() => location.href = '/pages/clientes.html', 1200);
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
  const email = campos.email.value.trim();
  if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) { marcarErro('email', 'E-mail inválido.'); ok = false; }
  return ok;
}

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  if (!validar()) return;

  const dto = {
    nome: campos.nome.value.trim(),
    telefone: campos.telefone.value.trim() || null,
    email: campos.email.value.trim() || null,
  };

  btnSalvar.disabled = true;
  btnSalvar.textContent = 'Salvando...';
  try {
    if (editando) await Api.put('/api/clientes/' + id, dto);
    else await Api.post('/api/clientes', dto);
    Ui.sucesso('Cliente salvo com sucesso.');
    setTimeout(() => location.href = '/pages/clientes.html', 700);
  } catch (err) {
    Ui.erro(err.message);
    btnSalvar.disabled = false;
    btnSalvar.textContent = 'Salvar';
  }
});

carregar();
