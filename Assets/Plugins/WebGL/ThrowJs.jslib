mergeInto(LibraryManager.library, {
  ThrowJsError: function () {
    throw new Error("TEST: JS error thrown");
  }
});
