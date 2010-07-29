;; Binding test
(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error))))

;; Helpers
(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))
(define read (fn (str) (. 'ls.Reader 'Read str)))
(define eval (fn (str) (. 'ls.Evaluator 'Eval str *env*)))
