<template>
  <aside class="w-64 bg-white shadow-md flex flex-col">
    <div class="p-4 text-xl font-bold">NFS-e 2026</div>
    <nav class="flex-1">
      <RouterLink to="/dashboard" class="block p-3 hover:bg-gray-100">Dashboard</RouterLink>
      
      <!-- NFS-e -->
      <div class="px-3 py-2">
        <h3 class="text-xs font-semibold text-gray-500 uppercase tracking-wider">NFS-e</h3>
      </div>
      <RouterLink to="/nfse/nova" class="block p-3 hover:bg-gray-100 pl-6">Nova NFS-e</RouterLink>
      <RouterLink to="/nfse/lista" class="block p-3 hover:bg-gray-100 pl-6">NFS-e Emitidas</RouterLink>
      
      <!-- NF-e -->
      <div class="px-3 py-2 mt-4">
        <h3 class="text-xs font-semibold text-gray-500 uppercase tracking-wider">NF-e</h3>
      </div>
      <RouterLink to="/nfe/emitir" class="block p-3 hover:bg-gray-100 pl-6">Emitir NF-e</RouterLink>
      <RouterLink to="/nfe/lista" class="block p-3 hover:bg-gray-100 pl-6">NF-e Emitidas</RouterLink>
      <RouterLink to="/nfe/danfe-versao" class="block p-3 hover:bg-gray-100 pl-6">🎯 DANFE Versões</RouterLink>
      <RouterLink to="/nfe/configuracao" class="block p-3 hover:bg-gray-100 pl-6">Config. NF-e</RouterLink>
      
      <!-- Configurações -->
      <div class="px-3 py-2 mt-4">
        <h3 class="text-xs font-semibold text-gray-500 uppercase tracking-wider">Sistema</h3>
      </div>
      <RouterLink to="/configuracoes" class="block p-3 hover:bg-gray-100 pl-6">Configurações</RouterLink>
    </nav>
  </aside>
</template>