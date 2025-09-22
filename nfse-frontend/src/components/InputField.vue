<template>
  <div>
    <label class="block text-sm font-medium mb-1">{{ label }}</label>
    <input
      :type="type"
      :value="modelValue"
      @input="$emit('update:modelValue', $event.target.value)"
      class="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring focus:border-blue-300"
    />
  </div>
</template>

<script setup>
defineProps({
  label: String,
  type: { type: String, default: "text" },
  modelValue: [String, Number]
})
defineEmits(["update:modelValue"])
</script>